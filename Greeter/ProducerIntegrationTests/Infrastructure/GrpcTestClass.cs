#nullable enable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProducerIntegrationTests.Infrastructure
{
    public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception exception);

    public class GrpcTestClass<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;
        private readonly IHost _host;

        public event LogMessage? LoggedMessage;

        public GrpcTestClass(Action<IServiceCollection>? initialConfigureServices)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
            {
                LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
            }));

            var builder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    initialConfigureServices?.Invoke(services);
                    services.AddSingleton<ILoggerFactory>(LoggerFactory);
                })
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost
                        .UseTestServer()
                        .UseStartup<TStartup>();
                });
            _host = builder.Start();
            _server = _host.GetTestServer();

            // Need to set the response version to 2.0.
            // Required because of this TestServer issue - https://github.com/aspnet/AspNetCore/issues/16940
            var responseVersionHandler = new ResponseVersionHandler { InnerHandler = _server.CreateHandler() };

            var client = new HttpClient(responseVersionHandler) { BaseAddress = new Uri("http://localhost") };

            Client = client;
        }

        public LoggerFactory LoggerFactory { get; }

        public HttpClient Client { get; }

        public void Dispose()
        {
            Client.Dispose();
            _host.Dispose();
            _server.Dispose();
        }

        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;

                return response;
            }
        }

        public IDisposable GetTestContext()
        {
            return new GrpcTestContext<TStartup>(this);
        }
    }
}
