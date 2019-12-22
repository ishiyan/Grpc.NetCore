using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ProducerIntegrationTests.Infrastructure
{
    internal class GrpcTestContext<TStartup> : IDisposable where TStartup : class
    {
        private readonly ExecutionContext _executionContext;
        private readonly Stopwatch _stopwatch;
        private readonly GrpcTestClass<TStartup> _testClass;

        public GrpcTestContext(GrpcTestClass<TStartup> testClass)
        {
            _executionContext = ExecutionContext.Capture()!;
            _stopwatch = Stopwatch.StartNew();
            _testClass = testClass;
            _testClass.LoggedMessage += WriteMessage;
        }

        private void WriteMessage(LogLevel logLevel, string category, EventId eventId, string message, Exception exception)
        {
            // Log using the passed in execution context.
            // In the case of NUnit, console output is only captured by the test
            // if it is written in the test's execution context.
            ExecutionContext.Run(_executionContext, s =>
            {
                Console.WriteLine($"{_stopwatch.Elapsed.TotalSeconds:N3}s {category} - {logLevel}: {message}");
            }, null);
        }

        public void Dispose()
        {
            _testClass.LoggedMessage -= WriteMessage;
            _executionContext?.Dispose();
        }
    }
}
