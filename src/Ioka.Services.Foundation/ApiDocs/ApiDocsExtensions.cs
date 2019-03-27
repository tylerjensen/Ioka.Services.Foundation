using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ioka.Services.Foundation.ApiDocs
{
    public static class ApiDocsExtensions
    {
        /// <summary>
        /// Adds Swagger API documentation service.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="title"></param>
        /// <param name="version"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static IServiceCollection AddIokaApiDocs(this IServiceCollection services,
            string title = null, string version = "v1", string description = null, string contactName = null, string contactUrl = null,
            string license = null, string licenseUrl = null, string termsOfServiceUrl = null)
        {
            var swaggerHelper = new SwaggerHelper(title, version, description, contactName, contactUrl, license, licenseUrl, termsOfServiceUrl);
            services.AddSwaggerGen(swaggerHelper.ConfigureSwaggerGen);
            return services;
        }


        /// <summary>
        /// Adds Swagger API documentation UI by default at /api-docs
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routeTemplate"></param>
        /// <param name="routePrefix"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseIokaApiDocsUI(this IApplicationBuilder app,
            string routeTemplate = null, string routePrefix = null, string endpointUrl = null, string description = null)
        {
            var swaggerUIHelper = new SwaggerUIHelper(routeTemplate, routePrefix, endpointUrl, description);
            app.UseSwagger(swaggerUIHelper.ConfigureSwagger);
            app.UseSwaggerUI(swaggerUIHelper.ConfigureSwaggerUI);
            return app;
        }

    }
}
