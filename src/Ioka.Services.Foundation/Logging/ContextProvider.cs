using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Ioka.Services.Foundation.Logging
{
    public class ContextProvider
    {
        private IHttpContextAccessor _accessor;
        public ContextProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public HttpContext GetCurrent()
        {
            return _accessor.HttpContext;
        }
    }
}
