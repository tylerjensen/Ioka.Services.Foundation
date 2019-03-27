using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ioka.Services.IdentityServer
{
    public static class IdentityExtensions
    {
        public static int GetLifetimeInSeconds(this DateTime creationTime, DateTime now)
        {
            return ((int)(now - creationTime).TotalSeconds);
        }
    }
}
