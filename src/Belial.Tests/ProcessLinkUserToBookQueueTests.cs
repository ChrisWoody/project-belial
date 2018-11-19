using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessLinkUserToBookQueueTests
    {
        private const string LinkUserToBookQueueMessage = "{\"Title\":\"The Purging of Kadillus\",\"UserId\":\"12345\"}";

        [Fact]
        public async Task GivenMessageFromLinkUserToBookQueue_WhenProcessLinkUserToBookQueue_ThenUserBookAddedToUserBookTable()
        {
            var userToBookTable = new TestAsyncCollector<UserBookTableEntity>();

            await Functions.ProcessLinkUserToBookQueueFunction(LinkUserToBookQueueMessage, new TestLogger(), userToBookTable);

            Assert.Equal("The Purging of Kadillus", userToBookTable.QueuedItems[0].Title);
            Assert.Equal("12345", userToBookTable.QueuedItems[0].UserId);
        }
    }
}