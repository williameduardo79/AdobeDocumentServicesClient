using AdobeDocumentServicesClient.Enums;
using AdobeDocumentServicesClient.Models;
using Newtonsoft.Json.Linq;

namespace AdobeDocumentServicesClient.Services
{
    public interface IAdobeClientConnector
    {
        Task<CheckStatusResponse> CheckFileStatusAsync(AdobeToken adobeToken, MergeDocumentResponse documentResponse);
        Task<MergeDocumentResponse> DocumentGenerationAsync(JObject jasonObject, AdobeToken adobeToken, string assetId);
        Task<Stream> GetFileStreamAsync(string fileUrl);
        Task<AdobeToken> GetTokenAsync();
        Task<AssetResponse> GetUploadPreSignedUriAsync(AdobeToken adobeToken);
        Task<Stream> MergeDocumentAsync(Stream wordTemplate, JObject jsonObject);
        string SetAdobeURLRegion(RegionOptions regionOptions);
        string SetAdobeURLRegion(string overrideUrl);
        Task UploadDocumentAsync(string uploadUri, Stream wordTemplate);
    }
}