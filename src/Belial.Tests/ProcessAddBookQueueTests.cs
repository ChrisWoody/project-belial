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
                Title = "The Purging of Kadillus",
                ImageFilename = "21e237d2-a901-4e7e-b58f-19602bc99313.jpg",
            },
            UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C"),
        };

        [Fact]
        public async Task GivenMessageFromAddBookQueue_WhenProcessAddBookQueue_ThenBookAddedToBookTable()
        {
            var bookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));

            await Functions.ProcessAddBookQueueFunction(_addBookQueueMessage, new TestLogger(), bookTable.Object);

            bookTable.VerifyExecuteAsync(e => e.OperationType == TableOperationType.InsertOrReplace);
            bookTable.VerifyExecuteAsync(e => e.Entity is BookTableEntity);
            bookTable.VerifyExecuteAsync(e => e.Entity.PartitionKey == _addBookQueueMessage.UserId.ToString());
            bookTable.VerifyExecuteAsync(e => e.Entity.RowKey == _addBookQueueMessage.Book.Isbn);
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).Isbn == _addBookQueueMessage.Book.Isbn);
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).Title == _addBookQueueMessage.Book.Title);
            bookTable.VerifyExecuteAsync(e => ((BookTableEntity) e.Entity).ImageFilename == _addBookQueueMessage.Book.ImageFilename);
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