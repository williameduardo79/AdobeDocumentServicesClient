using AdobeDocumentServicesClient.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdobeDocumentServicesClient.Services
{
    public interface IAdobeClientConnector
    {
        Task<AdobeToken> GetTokenAsync();
        Task<AssetResponse> GetUploadPreSignedUriAsync(AdobeToken adobeToken);
        Task UploadFileAsync(string templateName, Stream wordTemplate, string uploadUri);
        Task<MergeDocumentResponse> RequestPDFDocumentAsync(JObject jasonObject, AdobeToken adobeToken, AssetResponse assetResponse);

    }
}
