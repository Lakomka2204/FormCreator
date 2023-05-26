using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FCApi
{
    public class ConfigureSwaggerVersioning : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider descriptionProvider;

        public ConfigureSwaggerVersioning(IApiVersionDescriptionProvider descriptionProvider)
        {
            this.descriptionProvider = descriptionProvider;
        }
        public void Configure(string? name, SwaggerGenOptions options)
        {
            Configure(options);
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in descriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, CreateVersionInfo(desc));
            }
        }

        static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "Form Creator API",
                Version = description.ApiVersion.ToString(),
                Contact = new OpenApiContact()
                {
                    Name = "Web version",
                    Url = new Uri("https://localhost:7051"),
                }
            };
            if (description.IsDeprecated)
                info.Description += "// This API version has been deprecated.";
            return info;
        }
    }
}
