using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Ioka.Services.Foundation.ApiDocs
{
    // See https://jack-vanlightly.com/blog/2017/8/23/api-series-part-2-swagger
    public class SwaggerHelper
    {
        private readonly string _title, _version, _description, _contactName, _contactUrl, _license, _licenseUrl, _termsOfServiceUrl;

        public SwaggerHelper(string title, string version, string description, string contactName, string contactUrl, string license, string licenseUrl, string termsOfServiceUrl)
        {
            _title = title ?? "Ioka.Services.Foundation";
            _version = version ?? "v1";
            _description = description ?? "REST services for accessing the Ioka.Services.Foundation services.";
            _contactName = contactName ?? "Ioka";
            _contactUrl = contactUrl ?? "http://localhost/contactus";
            _license = license ?? "License";
            _licenseUrl = licenseUrl ?? "http://localhost/license";
            _termsOfServiceUrl = termsOfServiceUrl ?? "http://localhost/tos";
        }

        public void ConfigureSwaggerGen(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Add(_version,
                new Info
                {
                    Title = _title,
                    Version = _version,
                    Description = _description,
                    Contact = new Contact()
                    {
                        Name = _contactName,
                        Url = _contactUrl
                    },
                    License = new License()
                    {
                        Name = _license,
                        Url = _licenseUrl
                    },
                    TermsOfService = _termsOfServiceUrl
                });

            swaggerGenOptions.EnableAnnotations();

            //swaggerGenOptions.SwaggerGeneratorOptions = new SwaggerGeneratorOptions
            //{
            //    SwaggerDocs = new Dictionary<string, Info>()
            //    {
            //        {
            //            _version,
            //            new Info
            //            {
            //                Title = _title,
            //                Version = _version,
            //                Description = _description,
            //                Contact = new Contact()
            //                {
            //                    Name = _contactName,
            //                    Url = _contactUrl
            //                }
            //            }
            //        }
            //    },

            //};
        }
    }

    public class SwaggerUIHelper
    {
        private readonly string _routeTemplate, _routePrefix, _endpointUrl, _description, _version;

        public SwaggerUIHelper(string routeTemplate = null, string routePrefix = null, string endpointUrl = null, string description = null, string version = null)
        {
            _version = version ?? "v1";
            _routePrefix = routePrefix ?? "api-docs";
            _routeTemplate = routeTemplate ?? $"{_routePrefix}/{{documentName}}/swagger.json";
            _endpointUrl = endpointUrl ?? $"/{_routePrefix}/{_version}/swagger.json";
            _description = description ?? $"{_version} Documentation";
        }

        public void ConfigureSwagger(SwaggerOptions swaggerOptions)
        {
            swaggerOptions.RouteTemplate = _routeTemplate;
        }

        public void ConfigureSwaggerUI(SwaggerUIOptions swaggerUIOptions)
        {
            swaggerUIOptions.SwaggerEndpoint(_endpointUrl, _description);
            swaggerUIOptions.RoutePrefix = _routePrefix;
        }
    }
}
