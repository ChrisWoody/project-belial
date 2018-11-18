using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Belial.Tests.Core
{
    public class TestAsyncCollector<T> : IAsyncCollector<T>
    {
        private readonly IList<T> _queuedItems = new List<T>();

        public IReadOnlyList<T> QueuedItems => new ReadOnlyCollection<T>(_queuedItems);

        public Task AddAsync(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            _queuedItems.Add(item);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }
    }
}