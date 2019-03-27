using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ioka.Services.Foundation.Versioning
{
    [DataContract]
    public class VersionInfo
    {
        [DataMember(Order = 1)]
        public string Version { get; set; } = "0.0.0.0";
        [DataMember(Order = 2)]
        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;
        [DataMember(Order = 3)]
        public string DocsUrl { get; set; } = "https://localhost";
        [DataMember(Order = 4)]
        public string Description { get; set; } = "(no description provided)";
    }
}
