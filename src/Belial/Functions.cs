using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Belial
{
    public static class Functions
    {
        private const string BookEntryQueueName = "book-entry-queue";
        private const string AddBookQueueName = "add-book-queue";
        private const string LinkUserToBookQueueName = "link-user-to-book-queue";
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

                if (string.IsNullOrWhiteSpace(bookEntry?.Book?.Title))
                    return new BadRequestObjectResult("Invalid request to add book. 'Title' missing.");

                if (string.IsNullOrWhiteSpace(bookEntry.UserId))
                    return new BadRequestObjectResult("Invalid request to add book. 'UserId' missing.");

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
            [Queue(AddBookQueueName)] IAsyncCollector<AddBookQueueMessage> addBookQueue,
            [Queue(LinkUserToBookQueueName)] IAsyncCollector<LinkUserToBookQueueMessage> linkUserToBookQueue)
        {
            log.LogInformation("Process Book Entry Queue function called");

            await addBookQueue.AddAsync(new AddBookQueueMessage
            {
                Book = bookEntryQueueMessage.Book,
                ImageUrl = bookEntryQueueMessage.ImageUrl
            });

            await linkUserToBookQueue.AddAsync(new LinkUserToBookQueueMessage
            {
                UserId = bookEntryQueueMessage.UserId,
                Book = bookEntryQueueMessage.Book
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

            var upsertBookOp = TableOperation.InsertOrReplace(new BookTableEntity
            {
                PartitionKey = "1234",
                RowKey = addBookQueueMessage.Book.Title,
                Title = addBookQueueMessage.Book.Title
            });
            await bookTable.ExecuteAsync(upsertBookOp);

            await downloadBookImageQueue.AddAsync(new DownloadBookImageQueueMessage
            {
                Title = addBookQueueMessage.Book.Title,
                ImageUrl = addBookQueueMessage.ImageUrl,
                Filename = $"{Guid.NewGuid()}.jpg"
            });
        }

        [FunctionName("ProcessLinkUserToBookQueue")]
        public static async Task ProcessLinkUserToBookQueueFunction(
            [QueueTrigger(LinkUserToBookQueueName)] LinkUserToBookQueueMessage linkUserToBookQueueMessage,
            ILogger log,
            [Table("userbook")] CloudTable userBookTable)
        {
            log.LogInformation("Process Link User To Book Queue function called");

            var upsertUserBookOp = TableOperation.InsertOrReplace(new UserBookTableEntity
            {
                PartitionKey = "1234",
                RowKey = "123456",
                Book = linkUserToBookQueueMessage.Book,
                UserId = linkUserToBookQueueMessage.UserId
            });
            await userBookTable.ExecuteAsync(upsertUserBookOp);
        }

        internal static IStreamProvider StreamProvider = new StreamProvider();

        [FunctionName("ProcessDownloadBookImageQueue")]
        public static async Task ProcessDownloadBookImageQueueFunction(
            [QueueTrigger(DownloadBookImageQueueName)] DownloadBookImageQueueMessage downloadBookImageQueueMessage,
            ILogger log,
            [Table("bookimage")] CloudTable bookImageTable,
            [Blob("image-original/{Filename}", FileAccess.Write)] Stream imageBlobStream)
        {
            log.LogInformation("Process Download Book Image Queue function called");
            var imageStream = await StreamProvider.GetStreamAsync(downloadBookImageQueueMessage.ImageUrl);

            await imageStream.CopyToAsync(imageBlobStream);

            var upsertBookImageOp = TableOperation.InsertOrReplace(new BookImageTableEntity
            {
                PartitionKey = "0",
                RowKey = "123456",
                FullImageBlobPath = $"image-original/{downloadBookImageQueueMessage.Filename}"
            });
            await bookImageTable.ExecuteAsync(upsertBookImageOp);
        }
    }

    public class Book
    {
        public string Title { get; set; }
    }

    public class BookEntryHttpMessage
    {
        public Book Book { get; set; }
        public string UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BookEntryQueueMessage
    {
        public Book Book { get; set; }
        public string UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class AddBookQueueMessage
    {
        public Book Book { get; set; }
        public string ImageUrl { get; set; }
    }

    public class LinkUserToBookQueueMessage
    {
        public Book Book { get; set; }
        public string UserId { get; set; }
    }
    
    public class BookTableEntity : TableEntity // to consider, have this inherit Book and just define PartitionKey and RowKey
    {
        public string Title { get; set; }
    }

    public class UserBookTableEntity : TableEntity
    {
        public Book Book { get; set; }
        public string UserId { get; set; }
    }

    public class DownloadBookImageQueueMessage
    {
        public string Title { get; set; } // the 'id' for now
        public string ImageUrl { get; set; }
        public string Filename { get; set; }
    }

    public class BookImageTableEntity : TableEntity
    {
        public string FullImageBlobPath { get; set; }
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
