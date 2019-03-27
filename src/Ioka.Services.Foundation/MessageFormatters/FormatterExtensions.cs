using System;
using System.Collections.Generic;
using System.Text;
using WebApiContrib.Core.Formatter.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace Ioka.Services.Foundation.MessageFormatters
{
    public static class FormatterExtensions
    {
        /// <summary>
        /// Adds ProtoBuf Formatters to ASP.NET framework.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IMvcBuilder AddIokaFormatters(this IMvcBuilder services)
        {
            //see https://damienbod.com/2017/06/30/using-protobuf-media-formatters-with-asp-net-core/
            services.AddProtobufFormatters();
            return services;
        }
    }
}
