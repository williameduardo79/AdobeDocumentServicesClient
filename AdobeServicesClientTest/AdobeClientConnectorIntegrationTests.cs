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
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;

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
            var token = await _setup.Connector.GetTokenAsync();

            // Assert
            Assert.AreEqual(token.ResponseCode, HttpStatusCode.OK);
            Assert.NotNull(token);
            Assert.NotNull(token.AccessToken);
        }
        
     
        [Test]
        public async Task Test_DocumentMerge_Success()
        {
            var connector = _setup.Connector;
            
           
            string filename = "fax.docx";
            if (!File.Exists(filename))
                Assert.Fail("File does not exist");
            FileStream fileStream = File.OpenRead(filename);
            fileStream.Seek(0, SeekOrigin.Begin);
            TestContactData contactData = new TestContactData("William", "Mendoza", "Will@somedomain.com", "123456789");
            string json = JsonConvert.SerializeObject(contactData);
            JObject jObject = JObject.Parse(json);
            var mergeDocumentResponse = await connector.MergeDocumentAsync(fileStream, jObject);
            // Assert.DoesNotThrowAsync(() => connector.UploadFileAsync(filename, fileStream, asset.uploadUri));

                       
            //Request the merge
            
            Assert.IsNotNull(mergeDocumentResponse);
            //Assert.IsTrue(mergeDocumentResponse.Length >  0);
            using (var pdfStream = new FileStream("StreamedFile.pdf", FileMode.Create, FileAccess.Write))
            {
              
                await mergeDocumentResponse.CopyToAsync(pdfStream);
            }
            fileStream.Dispose();
            mergeDocumentResponse.Dispose();

        }
    }
}