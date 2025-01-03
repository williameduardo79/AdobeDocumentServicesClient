﻿using AdobeDocumentServicesClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdobeDocumentServicesClient.Models
{
    public class AdobeCredentials
    {
        public const string Name = "AdobeCredentials";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public RegionOptions Region {  get; set; }
    }
}
