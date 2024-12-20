using AdobeDocumentServicesClient.Enums;
using AdobeDocumentServicesClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
      : this(httpClient, logger, options.Value.ClientId, options.Value.ClientSecret, options.Value.Region, null)
        {
        }

        public AdobeClientConnector(
            IHttpClientFactory httpClient,
            ILogger logger,
            string clientId,
            string clientSecret,
            RegionOptions region)
            : this(httpClient, logger, clientId, clientSecret, region, region == RegionOptions.Custom ? throw new ValidationException("Custom Region Options require a custom URL") : null)
        {
        }

        public AdobeClientConnector(
            IHttpClientFactory httpClient,
            ILogger logger,
            string clientId,
            string clientSecret,
            RegionOptions region,
            string? customUrl)
        {
            _httpClientFactory = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (region == RegionOptions.Custom && string.IsNullOrWhiteSpace(customUrl))
            {
                throw new ValidationException("Custom Region Options require a custom URL");
            }

            if (region != RegionOptions.Custom && !string.IsNullOrWhiteSpace(customUrl))
            {
                throw new ValidationException("Use this constructor for Custom URL only (RegionOptions.Custom)");
            }

            _adobeCredentials = new AdobeCredentials
            {
                ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId)),
                ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret)),
                Region = region
            };

            _adobeUrl = region == RegionOptions.Custom ? customUrl! : SetAdobeURLRegion(region);
        }
        public string SetAdobeURLRegion(RegionOptions regionOptions)
        {
            _adobeUrl = AdobeURLRegion(regionOptions, "");
            return _adobeUrl;
        }
        public string SetAdobeURLRegion(string overrideUrl)
        {
            _adobeUrl = AdobeURLRegion(RegionOptions.Custom, overrideUrl);
            return _adobeUrl;
        }
        /// <summary>
        /// Merges a document and throws an exception if it cannot generate the document. Make sure you dispose your streams when done.
        /// </summary>
        /// <param name="wordTemplate"></param>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<Stream> MergeDocumentAsync(Stream wordTemplate, JObject jsonObject)
        {
                var token = await GetTokenAsync();
                if (token.ResponseCode != System.Net.HttpStatusCode.OK)
                    throw new UnauthorizedAccessException("Could not get token");

                var assetResponse = await GetUploadPreSignedUriAsync(token);
                await UploadDocumentAsync(assetResponse.uploadUri, wordTemplate);
                var mergeResponse = await DocumentGenerationAsync(jsonObject, token, assetResponse.assetID);
                var statusResponse = await CheckFileStatusAsync(token, mergeResponse);
                while (statusResponse.Status.ToLower() == "in progress")
                {
                //Waiting on Adobe service to merge file. If you want to avoid this step you may omit this method and run through each process.
                    Thread.Sleep(2000);

                    statusResponse = await CheckFileStatusAsync(token, mergeResponse);
                }
                if (statusResponse.Status.ToLower() == "done")
                {
                    var downloadUri = statusResponse.Asset.DownloadUri;
                    var downloadedFile = await GetFileStreamAsync(downloadUri);

                    return downloadedFile;
                }

            throw new Exception("Could not merge document. see logs");

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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", adobeToken.AccessToken);
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
        public async Task UploadDocumentAsync(string uploadUri, Stream wordTemplate)
        {

            if (wordTemplate == null)
                throw new ArgumentNullException(nameof(wordTemplate));

            if (string.IsNullOrWhiteSpace(uploadUri))
                throw new ArgumentException("Upload URI must be provided", nameof(uploadUri));

            // Ensure the stream is at the beginning
            if (wordTemplate.CanSeek)
            {
                wordTemplate.Seek(0, SeekOrigin.Begin);
            }
            // Initialize HttpClient
            var client = _httpClientFactory.CreateClient();
            // Create StreamContent for the file upload
            var content = new StreamContent(wordTemplate);
           
            // Set the Content-Type header correctly on the content object
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            // Create the HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Put, uploadUri)
            {
                Content = content
            };
          
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            // Log Request Headers
            _logger.LogInformation($"Request URL: {uploadUri}");
            _logger.LogInformation($"Request Method: {request.Method}");
            _logger.LogInformation("Request Headers:");
            foreach (var header in request.Headers)
            {
                _logger.LogInformation($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        
            // Send request
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            // Log Response
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response Status: {response.StatusCode}");
            _logger.LogInformation($"Response Body: {responseBody}");

        }
        public async Task<MergeDocumentResponse> DocumentGenerationAsync
          (
          JObject jasonObject,
          AdobeToken adobeToken,
          string assetId)
        {
            Stream outputStream = new MemoryStream();
            Stream? template = null;
            var address = urlConstructor("operation/documentgeneration");

            var clientId = _adobeCredentials.ClientId;
            var requestBody = new MergeDocumentRequest();
            requestBody.assetID = assetId;
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

            string headerResponseLocation = string.Empty;
            string headerResponseXRequest = string.Empty;
            if (response.Headers.Contains("location"))
            {
                headerResponseLocation = response.Headers.GetValues("location").FirstOrDefault();
            }
            if (response.Headers.Contains("x-request-id"))
            {
                headerResponseXRequest = response.Headers.GetValues("x-request-id").FirstOrDefault();
            }
            var result = await response.Content.ReadAsStringAsync();
            var documentResponse = new MergeDocumentResponse(headerResponseLocation, headerResponseXRequest);
            return documentResponse;
        }
        public async Task<CheckStatusResponse> CheckFileStatusAsync(AdobeToken adobeToken, MergeDocumentResponse documentResponse)
        {
            var clientId = _adobeCredentials.ClientId;

            // Initialize HttpClient
            var client = _httpClientFactory.CreateClient();
            // Make POST request to the API endpoint
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adobeToken.AccessToken);
            client.DefaultRequestHeaders.Add("x-api-key", clientId);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync(documentResponse.Location, HttpCompletionOption.ResponseHeadersRead);

            // Ensure success status code
            response.EnsureSuccessStatusCode();

            // Read and return the response content
            var responseString = await response.Content.ReadAsStringAsync();
            var statusResponse = JsonConvert.DeserializeObject<CheckStatusResponse>(responseString);

            return statusResponse ?? new CheckStatusResponse { Status = "Failed: Cant parse null object" };
        }
        public async Task<Stream> GetFileStreamAsync(string fileUrl)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // Return the response content as a stream
            return await response.Content.ReadAsStreamAsync();
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
                    apiUrl = "https://pdf-services-ue1.adobe.io";
                    break;
                case RegionOptions.Europe:
                    apiUrl = "https://pdf-services-ew1.adobe.io";
                    break;
                case RegionOptions.Custom:
                    apiUrl = overrideUrl;
                    break;

            }
            return apiUrl;
        }

    }
}
