using WebActivatorEx;
using Swashbuckle.Swagger;
using System.Web.Http.Description;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API
{
    public class AddFileUploadParams : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.operationId == "ProductSearchImageBased_ClassifyImage")
            {
                operation.consumes.Add("application/form-data");
                operation.parameters = new[]
                {
                    new Parameter
                    {
                        name = "imageFile",
                        @in = "formData",
                        description = "The file to upload.",
                        required = true,
                        type = "file"
                    },
            };
            }
        }
    }
}
