using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ioka.Services.IdentityServer
{
    public partial class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "Api 1"),
                new ApiResource("api2", "Api 2"),
                //cannot use openid as an api resource
            };
        }
    }
}
