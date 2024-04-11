using AdobeDocumentServicesClient.Enums;
using AdobeDocumentServicesClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace AdobeDocumentServicesClient.Services
{
    public class AdobeClientConnector : IAdobeClientConnector
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private AdobeCredentials _adobeCredentials;
        private string _adobeUrl;
        public AdobeClientConnector(IHttpClientFactory httpClient, ILogger logger, IOptions<AdobeCredentials> options)
        {
            _httpClientFactory = httpClient;
            _logger = logger;
            _adobeCredentials = options.Value;

        }
        public AdobeClientConnector(IHttpClientFactory httpClient, ILogger logger, string clientId, string clientSecret)
        {
            _httpClientFactory = httpClient;
            _logger = logger;
            _adobeCredentials = new AdobeCredentials();
            _adobeCredentials.ClientId = clientId;
            _adobeCredentials.ClientSecret = clientSecret;
        }
        public async Task<AdobeToken> GetTokenAsync()
        {
           
            AdobeToken adobeToken = new AdobeToken();
            //https://developer.adobe.com/document-services/docs/apis/#tag/Generate-Token

           
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _adobeCredentials.ClientId },
                { "client_secret", _adobeCredentials.ClientSecret }
				// Add more parameters if needed
			};
            string apiUrl = urlConstructor("token");
            var content = new FormUrlEncodedContent(parameters);

            // Initialize HttpClient
            var client = _httpClientFactory.CreateClient();

            // Make POST request to the API endpoint
            var response = await client.PostAsync(apiUrl, content);

            // Check if the request was successful

            if (response.IsSuccessStatusCode)
            {
                // Read the response content as string
                string responseContent = await response.Content.ReadAsStringAsync();

                // Handle the access token in the response
                adobeToken = JsonConvert.DeserializeObject<AdobeToken>(responseContent);


            }
            adobeToken.ResponseCode = response.StatusCode;

            return adobeToken;


        }
        public async Task<AssetResponse> GetUploadPreSignedUriAsync(AdobeToken adobeToken)
        {

            //https://developer.adobe.com/document-services/docs/apis/#tag/Generate-Token

            var clientId = _adobeCredentials.ClientId;
            string apiUrl = urlConstructor("assets");
            // Initialize HttpClient
            var client = _httpClientFactory.CreateClient();
            // Make POST request to the API endpoint
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adobeToken.AccessToken);
            client.DefaultRequestHeaders.Add("X-API-Key", clientId);
            // Define the request body
            var requestBody = new
            {
                mediaType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };
            // Serialize request body to JSON
            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            // Ensure success status code
            response.EnsureSuccessStatusCode();

            // Read and return the response content
            var responseString = await response.Content.ReadAsStringAsync();
            var assetResponse = JsonConvert.DeserializeObject<AssetResponse>(responseString);

            return assetResponse;

        }
        public async Task UploadFileAsync(string templateName, Stream wordTemplate, string uploadUri)
        {
           
            // Initialize HttpClient
            var client = _httpClientFactory.CreateClient();
            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

           
                ;
            using var content = new MultipartFormDataContent();
            content.Headers.ContentType = contentType;
            var template = wordTemplate;//Stream of the template 
            var fileContent = new StreamContent(template);
            content.Add(fileContent, "file", templateName);

            var fileResponse = await client.PutAsync(uploadUri, content);
            fileResponse.EnsureSuccessStatusCode();
        }
        public async Task<MergeDocumentResponse> RequestPDFDocumentAsync
          (
          JObject jasonObject,
          AdobeToken adobeToken,
          AssetResponse assetResponse)
        {
            Stream outputStream = new MemoryStream();
            Stream? template = null;
            var address = urlConstructor("documentgeneration"); 


            var clientId = _adobeCredentials.ClientId;
            var requestBody = new MergeDocumentRequest();
            requestBody.assetID = assetResponse.assetID;
            requestBody.outputFormat = "pdf";
            requestBody.jsonDataForMerge = jasonObject;



            // Initialize HttpClient
            var client = _httpClientFactory.CreateClient();
            // Make POST request to the API endpoint
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adobeToken.AccessToken);
            client.DefaultRequestHeaders.Add("x-api-key", clientId);
            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, address)
            {
                Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            // Ensure success status code
            response.EnsureSuccessStatusCode();


            var result = await response.Content.ReadAsStringAsync();
            var documentResponse = JsonConvert.DeserializeObject<MergeDocumentResponse>(result);
            return documentResponse;
        }
        private string urlConstructor(string endpoint)
        {
            if (String.IsNullOrEmpty(_adobeUrl))
            {
                throw new Exception("Must set the region by using the SetAdobeURLRegion method");
            }
            if (endpoint.EndsWith("/"))
            {
                return $"{_adobeUrl}{endpoint}";
            }
            else
            {
                return $"{_adobeUrl}/{endpoint}";
            }
        }
        private string AdobeURLRegion(RegionOptions regionOptions, string overrideUrl)
        {
            string apiUrl = "";
            switch (regionOptions)
            {
                case RegionOptions.UnitedStates:
                    apiUrl= "https://pdf-services-ue1.adobe.io";
                    break;
                case RegionOptions.Europe:
                    apiUrl = "https://pdf-services-ew1.adobe.io";
                    break;
                case RegionOptions.Other:
                    apiUrl = overrideUrl;
                    break;

            }
            return apiUrl;
        }
        public string SetAdobeURLRegion(RegionOptions regionOptions)
        {
            _adobeUrl = AdobeURLRegion(regionOptions, "");
            return _adobeUrl;
        }
        public string SetAdobeURLRegion(string overrideUrl)
        {
            _adobeUrl = AdobeURLRegion(RegionOptions.Other, overrideUrl);
            return _adobeUrl;
        }
    }
}
