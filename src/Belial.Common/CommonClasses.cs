using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Belial.Common
{
    public class Book
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string ImageFilename { get; set; }
    }

    public class BookEntryHttpMessage
    {
        public Book Book { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BooksForUser
    {
        public BookWithImage[] Books { get; set; }
    }

    public class BookWithImage : Book
    {
        public string FullImageUrl { get; set; }
    }

    public class BookEntryQueueMessage
    {
        public Book Book { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class AddBookQueueMessage
    {
        public Book Book { get; set; }
        public Guid UserId { get; set; }
    }

    public class BookTableEntity : TableEntity
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string ImageFilename { get; set; }
    }

    public class DownloadBookImageQueueMessage
    {
        public string ImageUrl { get; set; }
        public string Filename { get; set; }
    }
}
