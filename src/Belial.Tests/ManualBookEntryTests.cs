using System.Collections.Generic;
using System.Threading.Tasks;
using Belial.Tests.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;

namespace Belial.Tests
{
    public class ManualBookEntryTests
    {
        private const string ValidRequest = "{\"Book\":{\"Title\":\"The Purging of Kadillus\"},\"UserId\":\"12345\",\"ImageUrl\":\"https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg\"}";
        private readonly BookEntryHttpMessage _validRequestPoco = JsonConvert.DeserializeObject<BookEntryHttpMessage>(ValidRequest);

        [Theory, MemberData(nameof(InvalidRequests))]
        public async Task GivenAnInvalidRequest_WhenFunctionIsCalled_ThenBadRequestObjectResultIsReturned(string requestContent)
        {
            var request = TestHelper.CreateRequest(requestContent);

            var response = await Functions.ManualBookEntryFunction(request, new TestLogger(), new TestAsyncCollector<BookEntryQueueMessage>());

            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task GivenAValidRequest_WhenFunctionIsCalled_ThenOkObjectResultIsReturned()
        {
            var request = TestHelper.CreateRequest(ValidRequest);

            var response = await Functions.ManualBookEntryFunction(request, new TestLogger(), new TestAsyncCollector<BookEntryQueueMessage>());

            Assert.IsType<OkObjectResult>(response);
        }

        [Theory, MemberData(nameof(InvalidRequests))]
        public async Task GivenAnInvalidRequest_WhenFunctionIsCalled_ThenBookEntryQueueIsNotPopulated(string requestContent)
        {
            var queue = new TestAsyncCollector<BookEntryQueueMessage>();
            var request = TestHelper.CreateRequest(requestContent);

            await Functions.ManualBookEntryFunction(request, new TestLogger(), queue);

            Assert.Empty(queue.QueuedItems);
        }

        [Fact]
        public async Task GivenAValidRequest_WhenFunctionIsCalled_ThenBookEntryQueueIsPopulated()
        {
            var queue = new TestAsyncCollector<BookEntryQueueMessage>();
            var request = TestHelper.CreateRequest(ValidRequest);

            await Functions.ManualBookEntryFunction(request, new TestLogger(), queue);

            Assert.Equal(_validRequestPoco.Book.Title, queue.QueuedItems[0].Book.Title);
            Assert.Equal(_validRequestPoco.UserId, queue.QueuedItems[0].UserId);
            Assert.Equal(_validRequestPoco.ImageUrl, queue.QueuedItems[0].ImageUrl);
        }

        public static IEnumerable<object[]> InvalidRequests => new[]
        {
            new object[] {null},
            new object[] {""},
            new object[] {" "},
            new object[] {"{"},
            new object[] {"{asdaf}"},
            new object[]{"{\"Book\":{\"Title\":\"\"},\"UserId\":\"\",\"ImageUrl\":\"\"}"}, 
            new object[]{"{\"Book\":{\"Title\":\"The Purging of Kadillus\"},\"UserId\":\"\",\"ImageUrl\":\"\"}"}, 
            new object[]{"{\"Book\":{\"Title\":\"The Purging of Kadillus\"},\"UserId\":\"12345\",\"ImageUrl\":\"\"}"}, 
            new object[]{"{\"Book\":{\"Title\":\"The Purging of Kadillus\"},\"UserId\":\"\",\"ImageUrl\":\"https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg\"}"}, 
            new object[]{"{\"Book\":{\"Title\":\"\"},\"UserId\":\"12345\",\"ImageUrl\":\"https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg\"}"}, 
            new object[]{"{\"Book\":{\"Title\":\"\"},\"UserId\":\"\",\"ImageUrl\":\"https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg\"}"}, 
            // to consider, validate the url
        };
    }
}