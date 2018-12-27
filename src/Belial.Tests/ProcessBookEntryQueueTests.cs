using System;
using System.Threading.Tasks;
using Belial.Common;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessBookEntryQueueTests
    {
        private readonly BookEntryQueueMessage _bookEntryQueueMessage = new BookEntryQueueMessage
        {
            Book = new Book
            {
                Isbn = "9781844168965",
                Title = "The Purging of Kadillus",
                ImageFilename = null,
            },
            UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C"),
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
        };

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToAddBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<AddBookQueueMessage>();
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessBookEntryQueueFunction(_bookEntryQueueMessage, new TestLogger(), addBookQueue, downloadBookImageQueue);

            Assert.Equal(_bookEntryQueueMessage.Book.Isbn, addBookQueue.QueuedItems[0].Book.Isbn);
            Assert.Equal(_bookEntryQueueMessage.Book.Title, addBookQueue.QueuedItems[0].Book.Title);
            Assert.True(Guid.TryParse(addBookQueue.QueuedItems[0].Book.ImageFilename.Split('.')[0], out _));
            Assert.Equal("jpg", addBookQueue.QueuedItems[0].Book.ImageFilename.Split('.')[1]);
            Assert.Equal(_bookEntryQueueMessage.UserId, addBookQueue.QueuedItems[0].UserId);
        }

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenDownloadBookImageMessageAddedToQueue()
        {
            var addBookQueue = new TestAsyncCollector<AddBookQueueMessage>();
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessBookEntryQueueFunction(_bookEntryQueueMessage, new TestLogger(), addBookQueue, downloadBookImageQueue);

            Assert.Equal(_bookEntryQueueMessage.ImageUrl, downloadBookImageQueue.QueuedItems[0].ImageUrl);
            Assert.True(Guid.TryParse(downloadBookImageQueue.QueuedItems[0].Filename.Split('.')[0], out _));
            Assert.Equal("jpg", downloadBookImageQueue.QueuedItems[0].Filename.Split('.')[1]);
        }

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenBothQueueMessageHaveSameFilename()
        {
            var addBookQueue = new TestAsyncCollector<AddBookQueueMessage>();
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessBookEntryQueueFunction(_bookEntryQueueMessage, new TestLogger(), addBookQueue, downloadBookImageQueue);

            Assert.Equal(addBookQueue.QueuedItems[0].Book.ImageFilename, downloadBookImageQueue.QueuedItems[0].Filename);
        }
    }
}