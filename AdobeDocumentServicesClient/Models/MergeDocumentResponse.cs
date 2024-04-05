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
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("x-request-id")]
        public string RequestId { get; set; }
    }
}
