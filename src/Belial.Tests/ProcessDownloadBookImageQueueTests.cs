using System.Threading.Tasks;
using Belial.Tests.Core;
using Xunit;

namespace Belial.Tests
{
    public class ProcessDownloadBookImageQueueTests
    {
        private readonly DownloadBookImageQueueMessage _downloadBookImageQueueMessage = new DownloadBookImageQueueMessage
        {
            Title = "The Purging of Kadillus",
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
        };

        [Fact]
        public async Task GivenMessageFromDownloadBookImageQueue_WhenProcessDownloadBookImageQueue_ThenBookImageAddedToBookImageTable()
        {
            var bookImageTable = new TestAsyncCollector<BookImageTableEntity>();

            await Functions.ProcessDownloadBookImageQueueFunction(_downloadBookImageQueueMessage, new TestLogger(), bookImageTable);

            //Assert.True(_downloadBookImageQueueMessage.Title, bookImageTable.QueuedItems[0]);
        }
    }
}