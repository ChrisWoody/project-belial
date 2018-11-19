using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessBookEntryQueueTests
    {
        private const string BookEntryQueueMessage = "{\"Title\":\"The Purging of Kadillus\",\"UserId\":\"12345\"}";

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToAddBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<string>();
            var linkUserToBookQueue = new TestAsyncCollector<string>();

            await Functions.ProcessBookEntryQueueFunction(BookEntryQueueMessage, new TestLogger(), addBookQueue, linkUserToBookQueue);

            Assert.Equal(BookEntryQueueMessage, addBookQueue.QueuedItems[0]);
        }

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToLinkToUserBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<string>();
            var linkUserToBookQueue = new TestAsyncCollector<string>();

            await Functions.ProcessBookEntryQueueFunction(BookEntryQueueMessage, new TestLogger(), addBookQueue, linkUserToBookQueue);

            Assert.Equal(BookEntryQueueMessage, linkUserToBookQueue.QueuedItems[0]);
        }
    }
}