using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Greet;

namespace ProducerIntegrationTests
{
    [TestClass]
    public class GreeterServiceTests :  IntegrationTestBase
    {
        [TestMethod]
        public async Task GreeterService_SayHelloUnary_ReturnsCorrectResponse()
        {
            // Arrange
            var client = new Greeter.GreeterClient(Channel);

            // Act
            var response = await client.SayHelloUnaryAsync(new HelloRequest { Name = "Joe" });

            // Assert
            Assert.AreEqual("Hello Joe", response.Message);
        }

        [TestMethod]
        public async Task GreeterService_SayHelloServerStreaming_ReturnsCorrectResponse()
        {
            // Arrange
            var client = new Greeter.GreeterClient(Channel);
            var cts = new CancellationTokenSource();
            var hasMessages = false;
            var callCancelled = false;

            // Act
            using (var call = client.SayHelloServerStreaming(new HelloRequest { Name = "Joe" }, cancellationToken: cts.Token))
            {
                try
                {
                    // ReSharper disable once MethodSupportsCancellation
                    await foreach (var unused in call.ResponseStream.ReadAllAsync())
                    {
                        hasMessages = true;
                        cts.Cancel();
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    callCancelled = true;
                }
            }

            // Assert
            Assert.IsTrue(hasMessages);
            Assert.IsTrue(callCancelled);
        }

        [TestMethod]
        public async Task GreeterService_SayHelloClientStreaming_ReturnsCorrectResponse()
        {
            // Arrange
            var client = new Greeter.GreeterClient(Channel);
            var names = new[] { "James", "Jo", "Lee" };
            HelloReply response;

            // Act
            using (var call = client.SayHelloClientStreaming())
            {
                foreach (var name in names)
                {
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
                }

                await call.RequestStream.CompleteAsync();
                response = await call;
            }

            // Assert
            Assert.AreEqual("Hello James, Jo, Lee", response.Message);
        }

        [TestMethod]
        [Ignore("Bidirectional streaming is currently not supported in TestServer: https://github.com/aspnet/AspNetCore/pull/15591")]
        public async Task GreeterService_SayHelloBidirectionalStreaming_ReturnsCorrectResponse()
        {
            // Arrange
            var client = new Greeter.GreeterClient(Channel);
            var names = new[] { "James", "Jo", "Lee" };
            var messages = new List<string>();

            // Act
            using (var call = client.SayHelloBidirectionalStreaming())
            {
                foreach (var name in names)
                {
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name });

                    Assert.IsTrue(await call.ResponseStream.MoveNext());
                    messages.Add(call.ResponseStream.Current.Message);
                }

                await call.RequestStream.CompleteAsync();
            }

            // Assert
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Hello James", messages[0]);
        }
    }
}
