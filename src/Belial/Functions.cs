using System;
using System.IO;
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
        private const string LinkUserToBookQueueName = "link-user-to-book-queue";

        [FunctionName("ManualBookEntry")]
        public static async Task<IActionResult> ManualBookEntryFunction(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log,
            [Queue(BookEntryQueueName)] IAsyncCollector<string> bookEntryQueue)
        {
            log.LogInformation("Manual Book Entry function called");

            try
            {
                var bookEntryRequest = await new StreamReader(req.Body).ReadToEndAsync();
                var bookEntry = JsonConvert.DeserializeObject<BookEntry>(bookEntryRequest);

                if (string.IsNullOrWhiteSpace(bookEntry?.Title))
                    return new BadRequestObjectResult("Invalid request to add book. 'Title' missing.");

                if (string.IsNullOrWhiteSpace(bookEntry.UserId))
                    return new BadRequestObjectResult("Invalid request to add book. 'UserId' missing.");

                await bookEntryQueue.AddAsync(bookEntryRequest);

                return new OkObjectResult($"Valid request to add '{bookEntry.Title}'.");
            }
            catch (Exception e)
            {
                log.LogError(e, "An error occured");
                return new BadRequestObjectResult("An unknown error occured processing request");
            }
        }

        [FunctionName("ProcessBookEntryQueue")]
        public static async Task ProcessBookEntryQueueFunction(
            [QueueTrigger(BookEntryQueueName)] string bookEntryQueueMessage,
            ILogger log,
            [Queue(AddBookQueueName)] IAsyncCollector<string> addBookQueue,
            [Queue(LinkUserToBookQueueName)] IAsyncCollector<string> linkUserToBookQueue)
        {
            log.LogInformation("Process Book Entry Queue function called");

            var message = JsonConvert.DeserializeObject<BookEntry>(bookEntryQueueMessage);

            await addBookQueue.AddAsync(JsonConvert.SerializeObject(message));

            await linkUserToBookQueue.AddAsync(JsonConvert.SerializeObject(new LinkUserToBookQueueMessage
            {
                UserId = message.UserId,
                Title = message.Title
            }));
        }

        [FunctionName("ProcessAddBookQueue")]
        public static async Task ProcessAddBookQueueFunction(
            [QueueTrigger(AddBookQueueName)] string addBookQueueMessage,
            ILogger log,
            [Table("book")] IAsyncCollector<BookTableEntity> bookTable)
        {
            log.LogInformation("Process Add Book Queue function called");

            var bookEntry = JsonConvert.DeserializeObject<BookEntry>(addBookQueueMessage);

            // Assuming it doesn't exist for now
            await bookTable.AddAsync(new BookTableEntity
            {
                PartitionKey = "0",
                RowKey = "1234567",
                Title = bookEntry.Title,
            });
        }

        [FunctionName("ProcessLinkUserToBookQueue")]
        public static async Task ProcessLinkUserToBookQueueFunction(
            [QueueTrigger(LinkUserToBookQueueName)] string linkUserToBookQueueMessage,
            ILogger log,
            [Table("user-book")] IAsyncCollector<UserBookTableEntity> userBookTable)
        {
            log.LogInformation("Process Link User To Book Queue function called");

            var linkUserToBook = JsonConvert.DeserializeObject<LinkUserToBookQueueMessage>(linkUserToBookQueueMessage);

            await userBookTable.AddAsync(new UserBookTableEntity
            {
                PartitionKey = "0",
                RowKey = "123456",
                Title = linkUserToBook.Title,
                UserId = linkUserToBook.UserId
            });
        }
    }

    public class BookEntry
    {
        public string Title { get; set; }
        public string UserId { get; set; }
    }

    public class BookTableEntity : TableEntity
    {
        public string Title { get; set; }
    }

    public class LinkUserToBookQueueMessage
    {
        public string Title { get; set; }
        public string UserId { get; set; }
    }

    public class UserBookTableEntity : TableEntity
    {
        public string Title { get; set; }
        public string UserId { get; set; }
    }
}
