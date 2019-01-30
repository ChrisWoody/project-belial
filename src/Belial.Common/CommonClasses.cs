namespace Belial.Common
{
    public class Book
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string ImageFilename { get; set; }
        public bool HasRead { get; set; }
        public string Series { get; set; }
        public int? SeriesNumber { get; set; }
        public string[] AnthologyStories { get; set; }
        public string Type { get; set; }
        public string OriginalImageUrl { get; set; }
        public string FullImageUrl { get; set; }
    }

    public class DownloadImageQueueMessage
    {
        public string ImageUrl { get; set; }
        public string Filename { get; set; }
        public bool DontDownloadIfExists { get; set; }
    }

    public class RefreshImagesHttpMessage
    {
        public bool DontDownloadIfExists { get; set; }
        public string[] ImageUrlsToDownload { get; set; }
    }
}
