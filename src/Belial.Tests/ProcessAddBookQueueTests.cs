using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessAddBookQueueTests
    {
        private readonly AddBookQueueMessage _addBookQueueMessage = new AddBookQueueMessage
        {
            Book = new Book
            {
                Title = "The Purging of Kadillus"
            },
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
        };

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenBookAddedToBookTable()
        {
            var bookTable = new TestAsyncCollector<BookTableEntity>();
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessAddBookQueueFunction(_addBookQueueMessage, new TestLogger(), bookTable, downloadBookImageQueue);

            Assert.Equal(_addBookQueueMessage.Book.Title, bookTable.QueuedItems[0].Title);
        }

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenDownloadBookImageMessageAddedToQueue()
        {
            var bookTable = new TestAsyncCollector<BookTableEntity>();
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessAddBookQueueFunction(_addBookQueueMessage, new TestLogger(), bookTable, downloadBookImageQueue);

            Assert.Equal(_addBookQueueMessage.ImageUrl, downloadBookImageQueue.QueuedItems[0].ImageUrl);
        }
    }
}