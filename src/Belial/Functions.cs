using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Belial
{
    public static class Functions
    {
        [FunctionName("ManualBookEntry")]
        public static async Task<IActionResult> ManualBookEntryFunction(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Manual Book Entry function called");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var book = JsonConvert.DeserializeObject<BookEntry>(requestBody);

            return book?.Name != null
                ? (ActionResult)new OkObjectResult($"Valid request to add '{book.Name}'.")
                : new BadRequestObjectResult("Invalid request to add book. 'Name' missing.");
        }
    }

    public class BookEntry
    {
        public string Name { get; set; }
    }
}
