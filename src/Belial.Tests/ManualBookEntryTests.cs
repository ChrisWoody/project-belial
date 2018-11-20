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
        private const string ValidRequest = "{\"Book\":{\"Title\":\"The Purging of Kadillus\"},\"UserId\":\"12345\"}";
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
        }

        public static IEnumerable<object[]> InvalidRequests => new[]
        {
            new object[] {null},
            new object[] {""},
            new object[] {" "},
            new object[] {"{"},
            new object[] {"{Title}"},
            new object[] {"{\"Title\"}"},
            new object[] {"{\"Title\":The Purging of Kadillus\"}"},
            new object[] {"{\"Title\":\"\"}"},
            new object[] {"{\"Title\":\" \"}"},
            new object[] {"{\"Title\":null}"},
            new object[] {"{\"Title\":null,\"UserId\":}"},
            new object[] {"{\"Title\":\"\",\"UserId\":}"},
            new object[] {"{\"Title\":\"The Purging of Kadillus\",\"UserId\":\"\"}"},
            new object[] {"{\"Title\":\"The Purging of Kadillus\",\"UserId\":\" \"}"},
            new object[] {"{\"Title\":\"The Purging of Kadillus\",\"UserId\":null}"},
            new object[] {"{\"BookName\":\"The Purging of Kadillus\"}"}
        };
    }
}