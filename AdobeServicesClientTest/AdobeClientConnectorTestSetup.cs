using AdobeDocumentServicesClient.Enums;
using AdobeDocumentServicesClient.Models;
using AdobeDocumentServicesClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
      

        [OneTimeSetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Don't make it optional
            .AddUserSecrets<AdobeClientConnectorIntegrationTests>() // This tells the application to load User Secrets.
            .Build();

            var services = new ServiceCollection();

            services.AddHttpClient();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });
            AdobeCredentials config = configuration.GetSection("AdobeCredentials").Get<AdobeCredentials>(); ;
            services.Configure<AdobeCredentials>(configuration.GetSection("AdobeCredentials"));

            var serviceProvider = services.BuildServiceProvider();

            _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _logger = serviceProvider.GetRequiredService<ILogger<AdobeClientConnector>>();
            _options = serviceProvider.GetRequiredService<IOptions<AdobeCredentials>>();

            _connector = new AdobeClientConnector(_httpClientFactory, _logger, _options);
            _connector.SetAdobeURLRegion(RegionOptions.UnitedStates);

        }

        public AdobeClientConnector Connector => _connector;
      
    }

}
