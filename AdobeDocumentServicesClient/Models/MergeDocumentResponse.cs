using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdobeDocumentServicesClient.Models
{
    public class MergeDocumentResponse
    {
        public MergeDocumentResponse(string location, string request)
        {
            Location = location;
            RequestId = request;
        }
        [JsonProperty("location")]
        public string Location { get; init; }
        [JsonProperty("x-request-id")]
        public string RequestId { get; init; }
    }
}
