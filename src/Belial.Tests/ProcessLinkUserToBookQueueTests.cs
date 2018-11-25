using System;
using System.Threading.Tasks;
using Belial.Tests.Core;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Belial.Tests
{
    public class ProcessLinkUserToBookQueueTests
    {
        private readonly LinkUserToBookQueueMessage _linkUserToBookQueueMessage = new LinkUserToBookQueueMessage
        {
            Book = new Book
            {
                Title = "The Purging of Kadillus",
            },
            UserId = "12345"
        };

        [Fact]
        public async Task GivenMessageFromLinkUserToBookQueue_WhenProcessLinkUserToBookQueue_ThenUserBookAddedToUserBookTable()
        {
            var userToBookTable = new Mock<CloudTable>(new Uri("https://testurl.com"));

            await Functions.ProcessLinkUserToBookQueueFunction(_linkUserToBookQueueMessage, new TestLogger(), userToBookTable.Object);

            userToBookTable.VerifyExecuteAsync(e => e.OperationType == TableOperationType.InsertOrReplace);
            userToBookTable.VerifyExecuteAsync(e => e.Entity is UserBookTableEntity);
            userToBookTable.VerifyExecuteAsync(e => ((UserBookTableEntity) e.Entity).UserId == _linkUserToBookQueueMessage.UserId);
            userToBookTable.VerifyExecuteAsync(e => ((UserBookTableEntity) e.Entity).Book.Title == _linkUserToBookQueueMessage.Book.Title);
        }
    }
}