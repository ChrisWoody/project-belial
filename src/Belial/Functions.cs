using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        [FunctionName("ProcessBookEntryQueue")]
        public static async Task ProcessBookEntryQueueFunction(
            [QueueTrigger(BookEntryQueueName)] BookEntryQueueMessage bookEntryQueueMessage,
            ILogger log,
            [Queue(AddBookQueueName)] IAsyncCollector<AddBookQueueMessage> addBookQueue)
        {
            log.LogInformation("Process Book Entry Queue function called");

            await addBookQueue.AddAsync(new AddBookQueueMessage
            {
                Book = bookEntryQueueMessage.Book,
                ImageUrl = bookEntryQueueMessage.ImageUrl,
                UserId = bookEntryQueueMessage.UserId
            });
        }

        [FunctionName("ProcessAddBookQueue")]
        public static async Task ProcessAddBookQueueFunction(
            [QueueTrigger(AddBookQueueName)] AddBookQueueMessage addBookQueueMessage,
            ILogger log,
            [Table("book")] CloudTable bookTable,
            [Queue(DownloadBookImageQueueName)] IAsyncCollector<DownloadBookImageQueueMessage> downloadBookImageQueue)
        {
            log.LogInformation("Process Add Book Queue function called");

            var imageExt = Path.GetExtension(addBookQueueMessage.ImageUrl);
            var imageFilename = $"{Guid.NewGuid()}{imageExt}";

            var upsertBookOp = TableOperation.InsertOrReplace(new BookTableEntity
            {
                PartitionKey = addBookQueueMessage.UserId.ToString(),
                RowKey = addBookQueueMessage.Book.Isbn,
                Isbn = addBookQueueMessage.Book.Isbn,
                Title = addBookQueueMessage.Book.Title,
                ImageFilename = imageFilename
            });
            await bookTable.ExecuteAsync(upsertBookOp);

            await downloadBookImageQueue.AddAsync(new DownloadBookImageQueueMessage
            {
                ImageUrl = addBookQueueMessage.ImageUrl,
                Filename = imageFilename
            });
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

    public class Book
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
    }

    public class BookEntryHttpMessage
    {
        public Book Book { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BookEntryQueueMessage
    {
        public Book Book { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class AddBookQueueMessage
    {
        public Book Book { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BookTableEntity : TableEntity // to consider, have this inherit Book and just define PartitionKey and RowKey
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string ImageFilename { get; set; }
    }

    public class DownloadBookImageQueueMessage
    {
        public string ImageUrl { get; set; }
        public string Filename { get; set; }
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
