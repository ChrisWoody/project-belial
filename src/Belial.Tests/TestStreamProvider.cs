using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Belial.Tests
{
    public class TestStreamProvider : IStreamProvider
    {
        private readonly string _streamContent;

        public TestStreamProvider(string streamContent)
        {
            _streamContent = streamContent;
        }

        public async Task<Stream> GetStreamAsync(string url)
        {
            return await Task.FromResult(new MemoryStream(Encoding.UTF8.GetBytes(_streamContent)));
        }
    }
}