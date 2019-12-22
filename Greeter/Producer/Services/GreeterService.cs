using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

using Greet;

namespace Producer.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHelloUnary(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"SayHelloUnary: sending greeting to {request.Name}");
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task SayHelloServerStreaming(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var i = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var message = $"How are you {request.Name}? {++i}";
                _logger.LogInformation($"SayHelloServerStreaming: sending greeting {message}.");

                await responseStream.WriteAsync(new HelloReply { Message = message });

                // Gotta look busy.
                await Task.Delay(1000);
            }
        }

        public override async Task<HelloReply> SayHelloClientStreaming(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            var names = new List<string>();
            await foreach (var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"SayHelloClientStreaming: accumulating name {message.Name}.");
                names.Add(message.Name);
            }

            _logger.LogInformation($"SayHelloClientStreaming: sending greeting to {string.Join(", ", names)}");
            return new HelloReply { Message = "Hello " + string.Join(", ", names) };
        }

        public override async Task SayHelloBidirectionalStreaming(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"SayHelloBidirectionalStreaming: sending greeting to {message.Name}.");
                await responseStream.WriteAsync(new HelloReply { Message = "Hello " + message.Name });
            }
        }
    }
}
