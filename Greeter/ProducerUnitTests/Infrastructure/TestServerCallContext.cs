#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace ProducerUnitTests.Infrastructure
{
    public class TestServerCallContext : ServerCallContext
    {
        public Metadata? ResponseHeaders { get; private set; }

        private TestServerCallContext(Metadata requestHeaders, CancellationToken cancellationToken)
        {
            RequestHeadersCore = requestHeaders;
            CancellationTokenCore = cancellationToken;
            ResponseTrailersCore = new Metadata();
            AuthContextCore = new AuthContext(string.Empty, new Dictionary<string, List<AuthProperty>>());
        }

        protected override string MethodCore => "MethodName";
        protected override string HostCore => "HostName";
        protected override string PeerCore => "PeerName";

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        protected override DateTime DeadlineCore { get; }

        protected override Metadata RequestHeadersCore { get; }
        protected override CancellationToken CancellationTokenCore { get; }
        protected override Metadata ResponseTrailersCore { get; }
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore { get; }

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
        {
            throw new NotSupportedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            if (ResponseHeaders != null)
            {
                throw new InvalidOperationException("Response headers have already been written.");
            }

            ResponseHeaders = responseHeaders;
            return Task.CompletedTask;
        }

        public static TestServerCallContext Create(Metadata? requestHeaders = null, CancellationToken cancellationToken = default)
        {
            return new TestServerCallContext(requestHeaders ?? new Metadata(), cancellationToken);
        }
    }
}
