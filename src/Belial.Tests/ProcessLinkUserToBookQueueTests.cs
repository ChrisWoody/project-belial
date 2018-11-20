using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessLinkUserToBookQueueTests
    {
        private readonly LinkUserToBookQueueMessage _linkUserToBookQueueMessage = new LinkUserToBookQueueMessage
        {
            Book = new Book
            {
                Title = "The Purging of Kadillus",
            },
            UserId = "12345"
        };

        [Fact]
        public async Task GivenMessageFromLinkUserToBookQueue_WhenProcessLinkUserToBookQueue_ThenUserBookAddedToUserBookTable()
        {
            var userToBookTable = new TestAsyncCollector<UserBookTableEntity>();

            await Functions.ProcessLinkUserToBookQueueFunction(_linkUserToBookQueueMessage, new TestLogger(), userToBookTable);

            Assert.Equal("The Purging of Kadillus", userToBookTable.QueuedItems[0].Book.Title);
            Assert.Equal("12345", userToBookTable.QueuedItems[0].UserId);
        }
    }
}