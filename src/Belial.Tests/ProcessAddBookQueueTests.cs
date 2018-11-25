using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
                Title = "The Purging of Kadillus"
            },
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
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).Title == _addBookQueueMessage.Book.Title);
        }

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenDownloadBookImageMessageAddedToQueue()
        {
            var bookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));
            var downloadBookImageQueue = new TestAsyncCollector<DownloadBookImageQueueMessage>();

            await Functions.ProcessAddBookQueueFunction(_addBookQueueMessage, new TestLogger(), bookTable.Object, downloadBookImageQueue);

            Assert.Equal(_addBookQueueMessage.ImageUrl, downloadBookImageQueue.QueuedItems[0].ImageUrl);
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