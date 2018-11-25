using System;
using System.IO;
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
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg",
            Filename = $"{Guid.NewGuid()}.jpg"
        };

        [Fact]
        public async Task GivenMessageFromDownloadBookImageQueue_WhenProcessDownloadBookImageQueue_ThenBookImageAddedToBookImageTable()
        {
            var bookImageTable = new TestAsyncCollector<BookImageTableEntity>();
            var streamProvider = new TestStreamProvider("test stream content");
            var memoryStream = new MemoryStream();

            Functions.StreamProvider = streamProvider;
            await Functions.ProcessDownloadBookImageQueueFunction(_downloadBookImageQueueMessage, new TestLogger(), bookImageTable, memoryStream);

            Assert.Equal($"image-original/{_downloadBookImageQueueMessage.Filename}", bookImageTable.QueuedItems[0].FullImageBlobPath);
        }

        [Fact]
        public async Task GivenMessageFromDownloadBookImageQueue_WhenProcessDownloadBookImageQueue_ThenBookImageAddedToBlobStorage()
        {
            var bookImageTable = new TestAsyncCollector<BookImageTableEntity>();
            var streamProvider = new TestStreamProvider("test stream content");
            var memoryStream = new MemoryStream();

            Functions.StreamProvider = streamProvider;
            await Functions.ProcessDownloadBookImageQueueFunction(_downloadBookImageQueueMessage, new TestLogger(), bookImageTable, memoryStream);

            memoryStream.Position = 0;
            var streamContent = await new StreamReader(memoryStream).ReadToEndAsync();
            Assert.Equal("test stream content", streamContent);
        }
    }
}