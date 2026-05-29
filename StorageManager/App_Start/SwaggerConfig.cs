using System.Web.Http;
using WebActivatorEx;
using StorageManager.Swagger;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(
    typeof(StorageManager.App_Start.SwaggerConfig),
    "Register")]

namespace StorageManager.App_Start
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion(
                        "v1",
                        "StorageManager API");

                    // JWT support
                    c.ApiKey(
                        "Authorization")
                        .Description(
                            "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'")
                        .Name("Authorization")
                        .In("header");

                    c.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                })
                .EnableSwaggerUi(c =>
                {
                    c.DocumentTitle("StorageManager Swagger UI");
                });
        }
    }
}