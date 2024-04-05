using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdobeDocumentServicesClient.Models
{
    public class MergeDocumentRequest
    {


        public string assetID { get; set; }
        public string outputFormat { get; set; }
        public JObject jsonDataForMerge { get; set; }
        public Notifier[] notifiers { get; set; }




        public class Notifier
        {
            public string type { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public string url { get; set; }
            public Headers headers { get; set; }
        }

        public class Headers
        {
            public string xapikey { get; set; }
            public string accesstoken { get; set; }
        }

    }
}

