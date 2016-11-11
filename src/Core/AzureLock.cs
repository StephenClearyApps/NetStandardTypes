using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace NetStandardTypes
{
    // TODO: For now, this is an infinite lease.
    public sealed class AzureLock
    {
        private readonly CloudBlob _blob;

        public AzureLock(CloudBlob blob)
        {
            _blob = blob;
        }

        public async Task<IDisposable> LockAsync()
        {
            return new Key(_blob, await _blob.AcquireLeaseAsync(Timeout.InfiniteTimeSpan));
        }

        private sealed class Key : IDisposable
        {
            private readonly CloudBlob _blob;
            private AccessCondition _leaseId;

            public Key(CloudBlob blob, string leaseId)
            {
                _blob = blob;
                _leaseId = new AccessCondition { LeaseId = leaseId };
            }

            public void Dispose()
            {
                var leaseId = Interlocked.CompareExchange(ref _leaseId, null, _leaseId);
                if (leaseId != null)
                    _blob.ReleaseLease(leaseId);
            }
        }
    }
}
