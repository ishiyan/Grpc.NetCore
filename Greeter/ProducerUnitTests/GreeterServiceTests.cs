using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Greet;
using Producer.Services;
using ProducerUnitTests.Infrastructure;

namespace ProducerUnitTests
{
    [TestClass]
    public class GreeterServiceTests
    {
        [TestMethod]
        public async Task GreeterService_SayHelloUnary_ReturnsCorrectResponse()
        {
            // Arrange
            var service = new GreeterService(new Logger<GreeterService>(new NullLoggerFactory()));

            // Act
            var response = await service.SayHelloUnary(new HelloRequest { Name = "Joe" }, TestServerCallContext.Create());

            // Assert
            Assert.AreEqual("Hello Joe", response.Message);
        }

        [TestMethod]
        public async Task GreeterService_SayHelloServerStreaming_ReturnsCorrectResponse()
        {
            // Arrange
            var service = new GreeterService(new Logger<GreeterService>(new NullLoggerFactory()));
            var cts = new CancellationTokenSource();
            var callContext = TestServerCallContext.Create(cancellationToken: cts.Token);
            var responseStream = new TestServerStreamWriter<HelloReply>(callContext);

            // Act
            var call = service.SayHelloServerStreaming(new HelloRequest { Name = "Joe" }, responseStream, callContext);

            // Assert
            Assert.IsFalse(call.IsCompletedSuccessfully, "Method should run until cancelled.");
            cts.Cancel();
            await call;
            responseStream.Complete();
            var allMessages = new List<HelloReply>();
            // ReSharper disable once UseCancellationTokenForIAsyncEnumerable
            await foreach (var message in responseStream.ReadAllAsync())
            {
                allMessages.Add(message);
            }

            Assert.IsTrue( allMessages.Count >= 1);
            Assert.AreEqual("How are you Joe? 1", allMessages[0].Message);
        }

        [TestMethod]
        public async Task GreeterService_SayHelloClientStreaming_ReturnsCorrectResponse()
        {
            // Arrange
            var service = new GreeterService(new Logger<GreeterService>(new NullLoggerFactory()));
            var callContext = TestServerCallContext.Create();
            var requestStream = new TestAsyncStreamReader<HelloRequest>(callContext);

            // Act
            var call = service.SayHelloClientStreaming(requestStream, callContext);
            requestStream.AddMessage(new HelloRequest { Name = "James" });
            requestStream.AddMessage(new HelloRequest { Name = "Jo" });
            requestStream.AddMessage(new HelloRequest { Name = "Lee" });
            requestStream.Complete();

            // Assert
            var response = await call;
            Assert.AreEqual("Hello James, Jo, Lee", response.Message);
        }

        [TestMethod]
        public async Task GreeterService_SayHelloBidirectionalStreaming_ReturnsCorrectResponse()
        {
            // Arrange
            var service = new GreeterService(new Logger<GreeterService>(new NullLoggerFactory()));
            var callContext = TestServerCallContext.Create();
            var requestStream = new TestAsyncStreamReader<HelloRequest>(callContext);
            var responseStream = new TestServerStreamWriter<HelloReply>(callContext);

            // Act
            var call = service.SayHelloBidirectionalStreaming(requestStream, responseStream, callContext);

            // Assert
            requestStream.AddMessage(new HelloRequest { Name = "James" });
            Assert.AreEqual("Hello James", (await responseStream.ReadNextAsync())!.Message);
            requestStream.AddMessage(new HelloRequest { Name = "Jo" });
            Assert.AreEqual("Hello Jo", (await responseStream.ReadNextAsync())!.Message);
            requestStream.AddMessage(new HelloRequest { Name = "Lee" });
            Assert.AreEqual("Hello Lee", (await responseStream.ReadNextAsync())!.Message);
            requestStream.Complete();
            await call;
            responseStream.Complete();
            Assert.IsNull(await responseStream.ReadNextAsync());
        }
    }
}
