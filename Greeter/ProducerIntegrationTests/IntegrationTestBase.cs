#nullable enable
using System;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ProducerIntegrationTests.Infrastructure;

namespace ProducerIntegrationTests
{
    public class IntegrationTestBase
    {
        private GrpcChannel? _channel;
        private IDisposable? _testContext;

        // ReSharper disable once RedundantDefaultMemberInitializer
        protected GrpcTestClass<Producer.Startup> TestClass { get; private set; } = default!;

        protected ILoggerFactory LoggerFactory => TestClass.LoggerFactory;

        protected GrpcChannel Channel => _channel ??= CreateChannel();

        protected GrpcChannel CreateChannel()
        {
            return GrpcChannel.ForAddress(TestClass.Client.BaseAddress, new GrpcChannelOptions
            {
                LoggerFactory = LoggerFactory,
                HttpClient = TestClass.Client
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        [ClassInitialize]
        public void ClassInitialize()
        {
            TestClass = new GrpcTestClass<Producer.Startup>(ConfigureServices);
        }

        [ClassCleanup]
        public void ClassCleanup()
        {
            TestClass.Dispose();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            if (TestClass == null)
                TestClass = new GrpcTestClass<Producer.Startup>(ConfigureServices);
            _testContext = TestClass.GetTestContext();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _testContext?.Dispose();
            _channel = null;
        }
    }
}