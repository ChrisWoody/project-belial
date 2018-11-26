using System;
using System.Threading.Tasks;
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
                Title = "The Purging of Kadillus",
            },
            UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C"),
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
        };

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToAddBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<AddBookQueueMessage>();

            await Functions.ProcessBookEntryQueueFunction(_bookEntryQueueMessage, new TestLogger(), addBookQueue);

            Assert.Equal(_bookEntryQueueMessage.Book.Title, addBookQueue.QueuedItems[0].Book.Title);
            Assert.Equal(_bookEntryQueueMessage.UserId, addBookQueue.QueuedItems[0].UserId);
            Assert.Equal(_bookEntryQueueMessage.ImageUrl, addBookQueue.QueuedItems[0].ImageUrl);
        }
    }
}