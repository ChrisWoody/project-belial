using System.Threading.Tasks;
using Belial.Common;
using Belial.Tests.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;

namespace Belial.Tests
{
    public class RefreshImagesTests
    {
        [Fact]
        public async Task GivenAValidRequest_WhenFunctionIsCalled_ThenDownloadImageQueueIsPopulated()
        {
            var queue = new TestAsyncCollector<DownloadImageQueueMessage>();
            var request = TestHelper.CreateRequest(JsonConvert.SerializeObject(new RefreshImagesHttpMessage
            {
                DontDownloadIfExists = true,
                ImageUrlsToDownload = new[]
                {
                    "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg",
                    "https://images-na.ssl-images-amazon.com/images/I/8147jfELDHL.jpg"
                }
            }));

            var response = await Functions.RefreshImagesFunction(request, new TestLogger(), queue);

            Assert.IsType<OkObjectResult>(response);
            Assert.Equal("https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg", queue.QueuedItems[0].ImageUrl);
            Assert.Equal("srxdscrqY0AfJcMXFV44tw==.jpg", queue.QueuedItems[0].Filename);
            Assert.True(queue.QueuedItems[0].DontDownloadIfExists);
            Assert.Equal("https://images-na.ssl-images-amazon.com/images/I/8147jfELDHL.jpg", queue.QueuedItems[1].ImageUrl);
            Assert.Equal("z8o4LOo1ux8nHFDBXTagoA==.jpg", queue.QueuedItems[1].Filename);
            Assert.True(queue.QueuedItems[1].DontDownloadIfExists);
        }
    }
}