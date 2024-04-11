using AdobeDocumentServicesClient.Enums;
using AdobeDocumentServicesClient.Models;
using AdobeDocumentServicesClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdobeServicesClientTest
{
    [SetUpFixture]
    public class AdobeClientConnectorTestSetup
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger<AdobeClientConnector> _logger;
        private IOptions<AdobeCredentials> _options;
        private AdobeClientConnector _connector;
        private AdobeToken _token;

        [OneTimeSetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("bd2d53f4-81a8-47d2-bc60-7d07005f8bbf")
                .Build();

            var services = new ServiceCollection();

            services.AddHttpClient();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });
            services.Configure<AdobeCredentials>(configuration.GetSection("AdobeCredentials"));

            var serviceProvider = services.BuildServiceProvider();

            _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _logger = serviceProvider.GetRequiredService<ILogger<AdobeClientConnector>>();
            _options = serviceProvider.GetRequiredService<IOptions<AdobeCredentials>>();

            _connector = new AdobeClientConnector(_httpClientFactory, _logger, _options);
            _connector.SetAdobeURLRegion(RegionOptions.UnitedStates);

            _token = _connector.GetTokenAsync().GetAwaiter().GetResult();

        }

        public AdobeClientConnector Connector => _connector;
        public AdobeToken Token => _token;
    }

}
