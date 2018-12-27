using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Belial.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Belial
{
    public static class Functions
    {
        private const string BookEntryQueueName = "book-entry-queue";
        private const string AddBookQueueName = "add-book-queue";
        private const string DownloadBookImageQueueName = "download-book-image-queue";

        [FunctionName("ManualBookEntry")]
        public static async Task<IActionResult> ManualBookEntryFunction(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log,
            [Queue(BookEntryQueueName)] IAsyncCollector<BookEntryQueueMessage> bookEntryQueue)
        {
            log.LogInformation("Manual Book Entry function called");

            try
            {
                var bookEntryRequest = await new StreamReader(req.Body).ReadToEndAsync();
                var bookEntry = JsonConvert.DeserializeObject<BookEntryHttpMessage>(bookEntryRequest);

                if (string.IsNullOrWhiteSpace(bookEntry?.Book?.Isbn))
                    return new BadRequestObjectResult("Invalid request to add book. 'Isbn' missing.");

                if (string.IsNullOrWhiteSpace(bookEntry?.Book?.Title))
                    return new BadRequestObjectResult("Invalid request to add book. 'Title' missing.");

                if (bookEntry.UserId == Guid.Empty)
                    return new BadRequestObjectResult("Invalid request to add book. 'UserId' is empty.");

                if (string.IsNullOrWhiteSpace(bookEntry.ImageUrl))
                    return new BadRequestObjectResult("Invalid request to add book. 'ImageUrl' missing.");

                await bookEntryQueue.AddAsync(new BookEntryQueueMessage
                {
                    Book = bookEntry.Book,
                    UserId = bookEntry.UserId,
                    ImageUrl = bookEntry.ImageUrl
                });

                return new OkObjectResult($"Valid request to add '{bookEntry.Book.Title}'.");
            }
            catch (Exception e)
            {
                log.LogError(e, "An error occured");
                return new BadRequestObjectResult("An unknown error occured processing request");
            }
        }

        [FunctionName("GetBooksForUser")]
        public static async Task<IActionResult> GetBooksForUserFunction(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetBooksForUser/{userId}")] HttpRequest req,
            string userId,
            ILogger log,
            [Table("book")] CloudTable bookTable)
        {
            log.LogInformation("Get Books For User function called");
            
            try
            {
                if (!Guid.TryParse(userId, out _))
                    return new BadRequestObjectResult("Invalid request get user books. 'userId' is empty.");

                var connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var blobEndpoint = CloudStorageAccount.Parse(connection).BlobEndpoint.ToString();

                var books = await GetBookTableEntitiesForUser(bookTable, userId);
                var booksWithImage = books.Select(b => new BookWithImage
                {
                    Isbn = b.Isbn,
                    Title = b.Title,
                    FullImageUrl = $"{blobEndpoint}/image-original/{b.ImageFilename}", // note container might default to no public access
                });

                var response = new BooksForUser
                {
                    Books = booksWithImage.ToArray()
                };

                return new OkObjectResult(JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                log.LogError(e, "An error occured");
                return new BadRequestObjectResult("An unknown error occurred processing request");
            }
        }

        private static async Task<List<BookTableEntity>> GetBookTableEntitiesForUser(CloudTable bookTable,
            string userId)
        {
            var query = new TableQuery<BookTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

            var books = new List<BookTableEntity>();
            TableContinuationToken token = null;

            do
            {
                var queryResult = await bookTable.ExecuteQuerySegmentedAsync(query, token);
                token = queryResult.ContinuationToken;

                books.AddRange(queryResult.Results);
            } while (token != null);

            return books;
        }

        [FunctionName("ProcessBookEntryQueue")]
        public static async Task ProcessBookEntryQueueFunction(
            [QueueTrigger(BookEntryQueueName)] BookEntryQueueMessage bookEntryQueueMessage,
            ILogger log,
            [Queue(AddBookQueueName)] IAsyncCollector<AddBookQueueMessage> addBookQueue,
            [Queue(DownloadBookImageQueueName)] IAsyncCollector<DownloadBookImageQueueMessage> downloadBookImageQueue)
        {
            log.LogInformation("Process Book Entry Queue function called");

            var imageExt = Path.GetExtension(bookEntryQueueMessage.ImageUrl);
            var imageFilename = $"{Guid.NewGuid()}{imageExt}";

            bookEntryQueueMessage.Book.ImageFilename = imageFilename;

            await addBookQueue.AddAsync(new AddBookQueueMessage
            {
                Book = bookEntryQueueMessage.Book,
                UserId = bookEntryQueueMessage.UserId
            });

            // if ImageUrl is null/missing, can assume image is downloaded? I.e. don't compute a new imageFilename
            await downloadBookImageQueue.AddAsync(new DownloadBookImageQueueMessage
            {
                ImageUrl = bookEntryQueueMessage.ImageUrl,
                Filename = imageFilename
            });
        }

        [FunctionName("ProcessAddBookQueue")]
        public static async Task ProcessAddBookQueueFunction(
            [QueueTrigger(AddBookQueueName)] AddBookQueueMessage addBookQueueMessage,
            ILogger log,
            [Table("book")] CloudTable bookTable)
        {
            log.LogInformation("Process Add Book Queue function called");

            var upsertBookOp = TableOperation.InsertOrReplace(new BookTableEntity
            {
                PartitionKey = addBookQueueMessage.UserId.ToString(),
                RowKey = addBookQueueMessage.Book.Isbn,
                Isbn = addBookQueueMessage.Book.Isbn,
                Title = addBookQueueMessage.Book.Title,
                ImageFilename = addBookQueueMessage.Book.ImageFilename,
            });
            await bookTable.ExecuteAsync(upsertBookOp);
        }

        internal static IStreamProvider StreamProvider = new StreamProvider();

        [FunctionName("ProcessDownloadBookImageQueue")]
        public static async Task ProcessDownloadBookImageQueueFunction(
            [QueueTrigger(DownloadBookImageQueueName)] DownloadBookImageQueueMessage downloadBookImageQueueMessage,
            ILogger log,
            [Blob("image-original/{Filename}", FileAccess.Write)] Stream imageBlobStream)
        {
            log.LogInformation("Process Download Book Image Queue function called");
            var imageStream = await StreamProvider.GetStreamAsync(downloadBookImageQueueMessage.ImageUrl);

            await imageStream.CopyToAsync(imageBlobStream);
        }
    }

    public class StreamProvider : IStreamProvider
    {
        public async Task<Stream> GetStreamAsync(string url)
        {
            var httpClient = new HttpClient();
            
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStreamAsync();
        }
    }

    public interface IStreamProvider
    {
        Task<Stream> GetStreamAsync(string url);
    }
}
