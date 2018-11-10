using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Belial.Tests
{
    public static class TestHelper
    {
        public static HttpRequest CreateRequest(string json)
        {
            var request = new TestHttpRequest
            {
                Method = "Post",
                ContentLength = json?.Length,
                ContentType = "application/json",
                Body = json != null ? new MemoryStream(Encoding.UTF8.GetBytes(json)) : null
            };
 
            return request;
        }
    }
}