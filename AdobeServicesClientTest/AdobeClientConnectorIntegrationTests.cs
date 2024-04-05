using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
using AdobeDocumentServicesClient.Services;
using AdobeDocumentServicesClient.Models;

namespace AdobeServicesClientTest
{
    [TestFixture]
    public class AdobeClientConnectorIntegrationTests
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger<AdobeClientConnector> _logger;
        private IOptions<AdobeCredentials> _options;

        [SetUp]
        public void Setup()
        {
            // Build configuration
            var configuration = new ConfigurationBuilder
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // Set up DI container
            var services = new ServiceCollection();

            // Register HttpClient and other services
            services.AddHttpClient();

            services.AddLogging(builder =>
            {
                builder.AddConsole(); // Add logging to console
            });

            services.Configure<AdobeCredentials>(configuration.GetSection("AdobeCredentials"));

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve dependencies from service provider
            _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _logger = serviceProvider.GetRequiredService<ILogger<AdobeClientConnector>>();
            _options = serviceProvider.GetRequiredService<IOptions<AdobeCredentials>>();
        }

        [Test]
        public async Task Test_GetTokenAsync_Success()
        {
            // Arrange
            var connector = new AdobeClientConnector(_httpClientFactory, _logger, _options);

            // Act
            var token = await connector.GetTokenAsync();

            // Assert
            Assert.NotNull(token);
            Assert.NotNull(token.AccessToken);
        }
    }
}