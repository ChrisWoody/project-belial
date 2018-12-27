using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Belial.Common;
using Belial.Tests.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Belial.Tests
{
    public class GetBooksForUserTests
    {
        private readonly BookTableEntity _bookTableEntity = new BookTableEntity
        {
            Isbn = "9781844168965",
            Title = "The Purging of Kadillus",
            ImageFilename = "6C9F8062-7E28-4FB6-A7A5-31D46BCA864C.jpg",
            HasRead = true
        };

        [Fact]
        public async Task GivenAnInvalidUserId_WhenFunctionIsCalled_ThenBadRequestObjectResultIsReturned()
        {
            var invalidUserId = "fdhk3456";
            var bookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));

            var response = await Functions.GetBooksForUserFunction(new TestHttpRequest(), invalidUserId, new TestLogger(), bookTable.Object);

            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task GivenAValidUserId_WhenFunctionIsCalled_ThenOkObjectResultIsReturned()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            var invalidUserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C").ToString();
            var bookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));
            var tableQuerySegment = BuildTableQuerySegment();

            bookTable.Setup(x =>
                    x.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<BookTableEntity>>(), It.IsAny<TableContinuationToken>()))
                .Returns(() => Task.FromResult(tableQuerySegment));

            var response = await Functions.GetBooksForUserFunction(new TestHttpRequest(), invalidUserId, new TestLogger(), bookTable.Object);

            var responseResult = (OkObjectResult) response;
            var booksForUser = JsonConvert.DeserializeObject<BooksForUser>((string) responseResult.Value);

            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(_bookTableEntity.Isbn, booksForUser.Books[0].Isbn);
            Assert.Equal(_bookTableEntity.Title, booksForUser.Books[0].Title);
            Assert.Equal($"http://127.0.0.1:10000/devstoreaccount1/image-original/{_bookTableEntity.ImageFilename}", booksForUser.Books[0].FullImageUrl);
            Assert.Equal(_bookTableEntity.ImageFilename, booksForUser.Books[0].ImageFilename);
            Assert.Equal(_bookTableEntity.HasRead, booksForUser.Books[0].HasRead);
        }

        // As magic as Moq is, it can't mock something with an internal constructor, so we use reflection instead!
        private TableQuerySegment<BookTableEntity> BuildTableQuerySegment()
        {
            var ctor = typeof(TableQuerySegment<BookTableEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(c => c.GetParameters().Length == 1);

            var tableQuerySegment = ctor.Invoke(new object[] {new List<BookTableEntity>(new[] {_bookTableEntity})})
                as TableQuerySegment<BookTableEntity>;

            return tableQuerySegment;
        }
    }
}