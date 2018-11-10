using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Belial.Tests
{
    public class ManualBookEntryTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("{")]
        [InlineData("{Name}")]
        [InlineData("{\"Name\"}")]
        [InlineData("{\"Name\":The Purging of Kadillus\"}")]
        [InlineData("{\"Name\":\"\"}")]
        [InlineData("{\"Name\":\" \"}")]
        [InlineData("{\"Name\":null}")]
        [InlineData("{\"BookName\":\"The Purging of Kadillus\"}")]
        public async Task GivenAnInvalidRequest_WhenFunctionIsCalled_ThenBadRequestObjectResultIsReturned(string requestContent)
        {
            var request = TestHelper.CreateRequest(requestContent);

            var response = await Functions.ManualBookEntryFunction(request, new TestLogger());

            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task GivenAValidRequest_WhenFunctionIsCalled_ThenOkObjectResultIsReturned()
        {
            var request = TestHelper.CreateRequest("{\"Name\":\"The Purging of Kadillus\"}");

            var response = await Functions.ManualBookEntryFunction(request, new TestLogger());

            Assert.IsType<OkObjectResult>(response);
        }
    }
}