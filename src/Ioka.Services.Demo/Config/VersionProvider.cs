using Ioka.Services.Foundation.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ioka.Services.Demo.Config
{
    public class VersionProvider : IVersionProvider
    {
        IDictionary<string, VersionInfo> data;

        public VersionProvider()
        {
            data = new Dictionary<string, VersionInfo>()
            {
                { "Ioka.Services.Demo", new VersionInfo { Version = Assembly.GetAssembly(typeof(VersionProvider)).GetName().Version.ToString() } }
            };
        }

        public IDictionary<string, VersionInfo> VersionData {
            get => data;
            set => data = value;
        }
    }
}
