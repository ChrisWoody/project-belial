using System;
using System.IO;
using System.Threading.Tasks;
using Belial.Tests.Core;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
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
            var bookImageTable = new Mock<CloudTable>(new Uri("https://testurl.com"));
            var streamProvider = new TestStreamProvider("test stream content");
            var memoryStream = new MemoryStream();

            Functions.StreamProvider = streamProvider;
            await Functions.ProcessDownloadBookImageQueueFunction(_downloadBookImageQueueMessage, new TestLogger(), bookImageTable.Object, memoryStream);

            bookImageTable.VerifyExecuteAsync(e => e.OperationType == TableOperationType.InsertOrReplace);
            bookImageTable.VerifyExecuteAsync(e => e.Entity is BookImageTableEntity);
            bookImageTable.VerifyExecuteAsync(e => ((BookImageTableEntity) e.Entity).FullImageBlobPath == $"image-original/{_downloadBookImageQueueMessage.Filename}");
        }

        [Fact]
        public async Task GivenMessageFromDownloadBookImageQueue_WhenProcessDownloadBookImageQueue_ThenBookImageAddedToBlobStorage()
        {
            var bookImageTable = new Mock<CloudTable>(new Uri("https://testurl.com"));
            var streamProvider = new TestStreamProvider("test stream content");
            var memoryStream = new MemoryStream();

            Functions.StreamProvider = streamProvider;
            await Functions.ProcessDownloadBookImageQueueFunction(_downloadBookImageQueueMessage, new TestLogger(), bookImageTable.Object, memoryStream);

            memoryStream.Position = 0;
            var streamContent = await new StreamReader(memoryStream).ReadToEndAsync();
            Assert.Equal("test stream content", streamContent);
        }
    }
}