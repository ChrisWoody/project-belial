using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessAddBookQueueTests
    {
        private const string AddBookQueueMessage = "{\"Title\":\"The Purging of Kadillus\"}";

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenBookAddedToBookTable()
        {
            var bookTable = new TestAsyncCollector<BookTableEntity>();

            await Functions.ProcessAddBookQueueFunction(AddBookQueueMessage, new TestLogger(), bookTable);

            Assert.Equal("The Purging of Kadillus", bookTable.QueuedItems[0].Title);
        }
    }
}