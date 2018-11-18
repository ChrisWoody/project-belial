using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessBookEntryQueueTests
    {
        private const string BookEntryQueueMessage = "{\"Title\":\"The Purging of Kadillus\"}";

        [Fact]
        public async Task GivenMessageFromBookEntryQueue_WhenProcessBookEntryQueue_ThenMessagePushedToAddBookQueue()
        {
            var addBookQueue = new TestAsyncCollector<string>();

            await Functions.ProcessBookEntryQueueFunction(BookEntryQueueMessage, new TestLogger(), addBookQueue);

            Assert.Equal(BookEntryQueueMessage, addBookQueue.QueuedItems[0]);
        }
    }
}