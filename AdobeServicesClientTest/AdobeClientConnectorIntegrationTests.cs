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
using AdobeDocumentServicesClient.Enums;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using AdobeServicesClientTest.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Pipes;

namespace AdobeServicesClientTest
{
    [TestFixture]
    public class AdobeClientConnectorIntegrationTests
    {
        private AdobeClientConnectorTestSetup _setup;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            _setup = new AdobeClientConnectorTestSetup();
            _setup.Setup();
        }

        [Test]
        public async Task Test_GetTokenAsync_Success()
        {
            // Arrange
            var token = _setup.Token;

            // Assert
            Assert.AreEqual(token.ResponseCode, HttpStatusCode.OK);
            Assert.NotNull(token);
            Assert.NotNull(token.AccessToken);
        }
        
        private async Task<AssetResponse> GetUploadPreSignedUriAsync_Test(AdobeClientConnector connector, AdobeToken token)
        {
       
            var preSignedUrl = await connector.GetUploadPreSignedUriAsync(token);

            return preSignedUrl;

        }
        [Test]
        public async Task Test_DocumentMerge_Success()
        {
            var connector = _setup.Connector;
            //Get Token
            var token = _setup.Token;
            //Get Upload URL
            var asset = await GetUploadPreSignedUriAsync_Test(connector, token);
            Assert.IsNotNull(asset);
            Assert.IsNotNull(asset.uploadUri);
            //Upload the file to the uploadURL
            string filename = "fax.docx";
            if (!File.Exists(filename))
                Assert.Fail("File does not exist");
            FileStream fileStream = File.OpenRead(filename);
            fileStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrowAsync(() => connector.UploadFileAsync(filename, fileStream, asset.uploadUri));
           
            //Create some class to merge with document
            TestContactData contactData = new TestContactData("William", "Mendoza", "williameduardo@hotmail.com", "123456789");
            string json = JsonConvert.SerializeObject(contactData);
            JObject jObject = JObject.Parse(json);
            //Request the merge
            var mergeDocumentResponse = await connector.RequestPDFDocumentAsync(jObject, token, asset);
            Assert.IsNotNull(mergeDocumentResponse);
            Assert.IsNotNull(mergeDocumentResponse.Location);



        }
    }
}