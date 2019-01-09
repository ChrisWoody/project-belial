using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Belial.Common;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Belial
{
    public static class Functions
    {
        private const string DownloadImageQueueName = "download-image-queue";

        [FunctionName("GetBooks")]
        public static async Task<IActionResult> GetBooksFunction(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetBooks/{spreadsheetId}")] HttpRequest req,
            ILogger log,
            string spreadsheetId)
        {
            log.LogInformation("Get Books function called");
            
            try
            {
                var connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var blobEndpoint = CloudStorageAccount.Parse(connection).BlobEndpoint.ToString();

                var books = await GetBooks(spreadsheetId);

                return new OkObjectResult(JsonConvert.SerializeObject(books));
            }
            catch (Exception e)
            {
                log.LogError(e, "An error occured");
                return new BadRequestObjectResult("An unknown error occurred processing request");
            }
        }

        private static async Task<List<Book>> GetBooks(string spreadsheetId)
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { SheetsService.Scope.SpreadsheetsReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("token.json", true));
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "project-belial"
            });

            var books = new List<Book>();
            var worksheets = service.Spreadsheets.Get(spreadsheetId).Execute().Sheets.Select(x => x.Properties.Title);
            foreach (var worksheet in worksheets.Where(x => x != "Read Me"))
            {
                var range = $"{worksheet}!A2:G1000";
                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);

                var response = await request.ExecuteAsync();

                foreach (var book in response.Values)
                {
                    int? seriesNumber = null;
                    if (int.TryParse((string)book[1], out var validSeriesNumber))
                        seriesNumber = validSeriesNumber;

                    books.Add(new Book
                    {
                        Title = (string)book[0],
                        Series = worksheet,
                        SeriesNumber = seriesNumber,
                        Type = (string)book[2],
                        Isbn = (string)book[3],
                        HasRead = ((string)book[4]) == "Yes",
                        AnthologyStories = book.Count > 5 && !string.IsNullOrWhiteSpace((string)book[5]) ? ((string)book[5]).Split('|') : null,
                        FullImageUrl = "" //OriginalImageUrl = book.Count > 6 ? ((string)book[6]) : null
                    });
                }
            }

            return books;
        }

        // refresh images function
        // could have options, ie. hard refresh everything (download everything again), soft refresh (download everything that doesn't exist, so function will need to check)

        internal static IStreamProvider StreamProvider = new StreamProvider();

        //[FunctionName("ProcessDownloadImageQueue")]
        //public static async Task ProcessDownloadImageQueueFunction(
        //    [QueueTrigger(DownloadImageQueueName)] DownloadImageQueueMessage downloadImageQueueMessage,
        //    ILogger log,
        //    [Blob("image-original/{Filename}", FileAccess.ReadWrite)] CloudBlockBlob blob) // need Read for Exists?
        //{
        //    log.LogInformation("Process Download Image Queue function called");
        //    var imageStream = await StreamProvider.GetStreamAsync(downloadImageQueueMessage.ImageUrl);

        //    // check message first, might want to force update regardless if it exists
        //    if (await blob.ExistsAsync())
        //    {
        //        return;
        //    }

        //    await blob.UploadFromStreamAsync(imageStream);
        //    //await imageStream.CopyToAsync(imageBlobStream);
        //}

        // blob trigger to a smaller image, likely different container
    }

    public class StreamProvider : IStreamProvider
    {
        public async Task<Stream> GetStreamAsync(string url)
        {
            var httpClient = new HttpClient();
            
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStreamAsync();
        }
    }

    public interface IStreamProvider
    {
        Task<Stream> GetStreamAsync(string url);
    }
}
