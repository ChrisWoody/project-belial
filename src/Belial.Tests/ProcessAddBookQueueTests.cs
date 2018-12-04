using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Belial.Common;
using Belial.Tests.Core;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Belial.Tests
{
    public class ProcessAddBookQueueTests
    {
        private readonly AddBookQueueMessage _addBookQueueMessage = new AddBookQueueMessage
        {
            Book = new Book
            {
                Isbn = "9781844168965",
                Title = "The Purging of Kadillus"
            },
            UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C"),
            ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
        };

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenBookAddedToBookTable()
        {
            var bookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessAddBookQueueFunction(_addBookQueueMessage, new TestLogger(), bookTable.Object, downloadBookImageQueue);

            bookTable.VerifyExecuteAsync(e => e.OperationType == TableOperationType.InsertOrReplace);
            bookTable.VerifyExecuteAsync(e => e.Entity is BookTableEntity);
            bookTable.VerifyExecuteAsync(e => e.Entity.PartitionKey == _addBookQueueMessage.UserId.ToString());
            bookTable.VerifyExecuteAsync(e => e.Entity.RowKey == _addBookQueueMessage.Book.Isbn);
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).Isbn == _addBookQueueMessage.Book.Isbn);
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).Title == _addBookQueueMessage.Book.Title);
            bookTable.VerifyExecuteAsync(e => !string.IsNullOrWhiteSpace(((BookTableEntity) e.Entity).ImageFilename));
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).ImageFilename.EndsWith(".jpg"));
        }

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenDownloadBookImageMessageAddedToQueue()
        {
            var bookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessAddBookQueueFunction(_addBookQueueMessage, new TestLogger(), bookTable.Object, downloadBookImageQueue);

            Assert.Equal(_addBookQueueMessage.ImageUrl, downloadBookImageQueue.QueuedItems[0].ImageUrl);
            Assert.True(Guid.TryParse(downloadBookImageQueue.QueuedItems[0].Filename.Split('.')[0], out _));
            Assert.Equal("jpg", downloadBookImageQueue.QueuedItems[0].Filename.Split('.')[1]);
        }
    }

    public static class TestEx
    {
        public static void VerifyExecuteAsync(this Mock<CloudTable> mock, Expression<Func<TableOperation, bool>> verifyFunc)
        {
            mock.Verify(x => x.ExecuteAsync(It.Is(verifyFunc)));
        }
    }
}