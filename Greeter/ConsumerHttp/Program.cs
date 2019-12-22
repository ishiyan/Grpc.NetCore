using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using Greet;

namespace ConsumerHttp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string name = args.Length > 0 ? args[0] : "ConsumerHttp";

            Console.WriteLine("Creating channel ...");
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:5000");

            Console.WriteLine("Creating invoker ...");
            var invoker = channel.Intercept(new ConsumerHttpLoggerInterceptor());

            Console.WriteLine("Creating client ...");
            // var client = new Greeter.GreeterClient(channel);
            var client = new Greeter.GreeterClient(invoker);

            await UnaryCall(client, name);
            await ServerStreamingCall(client, name);
            await ClientStreamingCall(client, name);
            await DuplexStreamingCall(client, name);

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static async Task UnaryCall(Greeter.GreeterClient client, string name)
        {
            Console.WriteLine("Making unary call ...");
            var reply = await client.SayHelloUnaryAsync(new HelloRequest { Name = name });
            Console.WriteLine("Greeting: " + reply.Message);
        }

        private static async Task ServerStreamingCall(Greeter.GreeterClient client, string name)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3.5));

            Console.WriteLine("Making server streaming call ...");
            using var call = client.SayHelloServerStreaming(new HelloRequest { Name = name }, cancellationToken: cts.Token);
            try
            {
                await foreach (var message in call.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    Console.WriteLine("Greeting: " + message.Message);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Server stream cancelled");
            }
        }

        private static async Task ClientStreamingCall(Greeter.GreeterClient client, string name)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3.5));

            Console.WriteLine("Making client streaming call ...");
            using var call = client.SayHelloClientStreaming();
            try
            {
                for (var i = 0; i < 3; ++i)
                {
                    Console.WriteLine($"Streaming {name + i}");
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name + i });
                    await Task.Delay(200, cts.Token);
                }

                await call.RequestStream.CompleteAsync();
                var response = await call;
                Console.WriteLine("Greeting: " + response.Message);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Client stream cancelled");
            }
        }

        private static async Task DuplexStreamingCall(Greeter.GreeterClient client, string name)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3.5));

            Console.WriteLine("Making duplex streaming call ...");
            using var call = client.SayHelloBidirectionalStreaming();
            try
            {
                for (var i = 0; i < 3; ++i)
                {
                    Console.WriteLine($"Streaming {name + i}");
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name + i });
                    await Task.Delay(200, cts.Token);
                }

                await call.RequestStream.CompleteAsync();
                await foreach (var message in call.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    Console.WriteLine("Greeting: " + message.Message);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Duplex stream cancelled");
            }
        }
    }
}

