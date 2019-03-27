using System.Collections.Generic;

namespace Ioka.Services.Foundation.Versioning
{
    public interface IVersionProvider
    {
        IDictionary<string, VersionInfo> VersionData { get; set; }
    }
}
