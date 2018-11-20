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
            UserId = "12345"
        };

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToAddBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<AddBookQueueMessage>();
            var linkUserToBookQueue = new TestAsyncCollector<LinkUserToBookQueueMessage>();

            await Functions.ProcessBookEntryQueueFunction(_bookEntryQueueMessage, new TestLogger(), addBookQueue, linkUserToBookQueue);

            Assert.Equal(_bookEntryQueueMessage.Book.Title, addBookQueue.QueuedItems[0].Book.Title);
        }

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToLinkToUserBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<AddBookQueueMessage>();
            var linkUserToBookQueue = new TestAsyncCollector<LinkUserToBookQueueMessage>();

            await Functions.ProcessBookEntryQueueFunction(_bookEntryQueueMessage, new TestLogger(), addBookQueue, linkUserToBookQueue);

            Assert.Equal(_bookEntryQueueMessage.Book.Title, linkUserToBookQueue.QueuedItems[0].Book.Title);
            Assert.Equal(_bookEntryQueueMessage.UserId, linkUserToBookQueue.QueuedItems[0].UserId);
        }
    }
}