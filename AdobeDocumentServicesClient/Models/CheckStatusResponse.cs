using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdobeDocumentServicesClient.Models
{
    public class CheckStatusResponse
    {

        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("asset")]
        public StatusAsset Asset { get; set; }


        public class StatusAsset
        {
            [JsonProperty("metadata")]
            public Metadata MetaData { get; set; }
            [JsonProperty("downloadUri")]
            public string DownloadUri { get; set; }
            [JsonProperty("assetID")]
            public string AssetId { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("size")]
            public int Size { get; set; }
        }

    }
}

