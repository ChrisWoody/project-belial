using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

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

                var books = await GetBooks(spreadsheetId, blobEndpoint);

                return new OkObjectResult(JsonConvert.SerializeObject(books));
            }
            catch (Exception e)
            {
                log.LogError(e, "An error occured");
                return new BadRequestObjectResult("An unknown error occurred processing request");
            }
        }

        private static async Task<List<Book>> GetBooks(string spreadsheetId, string blobEndpoint)
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

                    var originalImageUrl = book.Count > 6 ? ((string)book[6]) : null;
                    var fullImageUrl = originalImageUrl != null ? $"{blobEndpoint}/image-smaller/{GetImageHashName(originalImageUrl)}" : null;

                    books.Add(new Book
                    {
                        Title = (string)book[0],
                        Series = worksheet,
                        SeriesNumber = seriesNumber,
                        Type = (string)book[2],
                        Isbn = (string)book[3],
                        HasRead = ((string)book[4]) == "Yes",
                        AnthologyStories = book.Count > 5 && !string.IsNullOrWhiteSpace((string)book[5]) ? ((string)book[5]).Split('|') : null,
                        OriginalImageUrl = originalImageUrl,
                        FullImageUrl = fullImageUrl
                    });
                }
            }

            return books;
        }

        [FunctionName("RefreshImages")]
        public static async Task<IActionResult> RefreshImagesFunction(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "RefreshImages")]
            HttpRequest req,
            ILogger log,
            [Queue(DownloadImageQueueName)] IAsyncCollector<DownloadImageQueueMessage> downloadImageQueue)
        {
            log.LogInformation("Refresh Images function called");

            try
            {
                var refreshImagesRequest = await new StreamReader(req.Body).ReadToEndAsync();
                var imagesToDownload = JsonConvert.DeserializeObject<string[]>(refreshImagesRequest);

                foreach (var imageToDownload in imagesToDownload)
                {
                    await downloadImageQueue.AddAsync(new DownloadImageQueueMessage
                    {
                        Filename = GetImageHashName(imageToDownload),
                        ImageUrl = imageToDownload
                    });
                }

                return new OkObjectResult($"Valid request to refresh '{imagesToDownload.Length}' images.");
            }
            catch (Exception e)
            {
                log.LogError(e, "An error occured");
                return new BadRequestObjectResult("An unknown error occured processing request");
            }
        }

        private static readonly MD5 Md5 = MD5.Create();

        private static string GetImageHashName(string imageUrl)
        {
            var hashBytes = Md5.ComputeHash(Encoding.UTF8.GetBytes(imageUrl));
            var hash = Convert.ToBase64String(hashBytes).Replace("/", "0");
            var fileExtension = Path.GetExtension(imageUrl);
            return hash + fileExtension;
        }

        internal static IStreamProvider StreamProvider = new StreamProvider();

        // This will just overwrite an image even if it exists for now
        [FunctionName("ProcessDownloadImageQueue")]
        public static async Task ProcessDownloadImageQueueFunction(
            [QueueTrigger(DownloadImageQueueName)] DownloadImageQueueMessage downloadImageQueueMessage,
            ILogger log,
            [Blob("image-original/{Filename}", FileAccess.Write)] Stream imageBlobStream)
        {
            log.LogInformation("Process Download Image Queue function called");
            var imageStream = await StreamProvider.GetStreamAsync(downloadImageQueueMessage.ImageUrl);

            await imageStream.CopyToAsync(imageBlobStream);
        }

        [FunctionName("ProcessOriginalImage")]
        public static async Task ProcessOriginalImageFunction(
            [BlobTrigger("image-original/{filename}")] Stream originalImageStream,
            ILogger log,
            string filename,
            [Blob("image-smaller/{filename}", FileAccess.Write)] Stream smallerImageStream)
        {
            log.LogInformation("Process Original Image function called for: " + filename);

            var image = Image.Load(originalImageStream);
            image.Mutate(x => x.Resize(new ResizeOptions {Mode = ResizeMode.Max, Size = new Size(200, 320)}));
            image.SaveAsJpeg(smallerImageStream);
        }
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
