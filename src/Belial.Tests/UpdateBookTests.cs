using System.Collections.Generic;
using System.Threading.Tasks;
using Belial.Common;
using Belial.Tests.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;

namespace Belial.Tests
{
    public class UpdateBookTests
    {
        private const string ValidRequest = "{\"Book\":{\"Isbn\":\"9781844168965\",\"Title\":\"The Purging of Kadillus\",\"ImageFilename\":\"21e237d2-a901-4e7e-b58f-19602bc99313.jpg\",\"HasRead\":\"true\"},\"UserId\":\"63CDBDDD-CE8C-411D-BA1E-0174FA19C05C\"}";
        private readonly UpdateBookHttpMessage _validRequestPoco = JsonConvert.DeserializeObject<UpdateBookHttpMessage>(ValidRequest);

        [Theory, MemberData(nameof(InvalidRequests))]
        public async Task GivenAnInvalidRequest_WhenFunctionIsCalled_ThenBadRequestObjectResultIsReturned(string requestContent)
        {
            var request = TestHelper.CreateRequest(requestContent);

            var response = await Functions.UpdateBookForUserFunction(request, new TestLogger(), new TestAsyncCollector<AddBookQueueMessage>());

            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task GivenAValidRequest_WhenFunctionIsCalled_ThenOkObjectResultIsReturned()
        {
            var request = TestHelper.CreateRequest(ValidRequest);

            var response = await Functions.UpdateBookForUserFunction(request, new TestLogger(), new TestAsyncCollector<AddBookQueueMessage>());

            Assert.IsType<OkObjectResult>(response);
        }

        [Theory, MemberData(nameof(InvalidRequests))]
        public async Task GivenAnInvalidRequest_WhenFunctionIsCalled_ThenAddBookQueueIsNotPopulated(string requestContent)
        {
            var queue = new TestAsyncCollector<AddBookQueueMessage>();
            var request = TestHelper.CreateRequest(requestContent);

            await Functions.UpdateBookForUserFunction(request, new TestLogger(), queue);

            Assert.Empty(queue.QueuedItems);
        }

        [Fact]
        public async Task GivenAValidRequest_WhenFunctionIsCalled_ThenAddBookQueueIsPopulated()
        {
            var queue = new TestAsyncCollector<AddBookQueueMessage>();
            var request = TestHelper.CreateRequest(ValidRequest);

            await Functions.UpdateBookForUserFunction(request, new TestLogger(), queue);

            Assert.Equal(_validRequestPoco.Book.Isbn, queue.QueuedItems[0].Book.Isbn);
            Assert.Equal(_validRequestPoco.Book.Title, queue.QueuedItems[0].Book.Title);
            Assert.Equal(_validRequestPoco.Book.ImageFilename, queue.QueuedItems[0].Book.ImageFilename);
            Assert.Equal(_validRequestPoco.Book.HasRead, queue.QueuedItems[0].Book.HasRead);
            Assert.Equal(_validRequestPoco.UserId, queue.QueuedItems[0].UserId);
        }

        public static IEnumerable<object[]> InvalidRequests => new[]
        {
            new object[] {null},
            new object[] {""},
            new object[] {" "},
            new object[] {"{"},
            new object[] {"{asdaf}"},
            new object[]{"{\"Book\":{\"Isbn\":\"\",\"Title\":\"\"},\"UserId\":\"\"}"},
        };
    }
}