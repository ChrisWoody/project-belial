﻿using System.IO;
using System.Threading.Tasks;
using Belial.Common;
using Belial.Tests.Core;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Xunit;

namespace Belial.Tests
{
    public class ProcessDownloadImageQueueTests
    {
        private readonly DownloadImageQueueMessage _downloadImageQueueMessage = new DownloadImageQueueMessage
        {
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg",
            Filename = "srxdscrqY/AfJcMXFV44tw==.jpg"
        };

        //[Fact]
        //public async Task GivenMessageFromDownloadBookImageQueue_WhenProcessDownloadBookImageQueue_ThenBookImageAddedToBlobStorage()
        //{
        //    var streamProvider = new TestStreamProvider("test stream content");
        //    var memoryStream = new MemoryStream();
        //    var blob = new Mock<CloudBlockBlob>();

        //    Functions.StreamProvider = streamProvider;
        //    await Functions.ProcessDownloadImageQueueFunction(_downloadImageQueueMessage, new TestLogger(), blob.Object);

        //    memoryStream.Position = 0;
        //    var streamContent = await new StreamReader(memoryStream).ReadToEndAsync();
        //    Assert.Equal("test stream content", streamContent);
        //}
    }
}