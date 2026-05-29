using System.Collections.Generic;
using Swashbuckle.Swagger;

namespace StorageManager.Swagger
{
    public class AddAuthorizationHeaderParameterOperationFilter
        : IOperationFilter
    {
        public void Apply(
            Operation operation,
            SchemaRegistry schemaRegistry,
            System.Web.Http.Description.ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                operation.parameters =
                    new List<Parameter>();
            }

            operation.parameters.Add(
                new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    description = "Bearer token",
                    required = false,
                    type = "string"
                });
        }
    }
}