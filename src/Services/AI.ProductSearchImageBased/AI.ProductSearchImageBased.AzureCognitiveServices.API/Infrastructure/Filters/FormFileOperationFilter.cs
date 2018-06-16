using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Infrastructure.Filters
{
    /// <summary>
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/193
    /// </summary>
    public class FormFileOperationFilter : IOperationFilter
    {
        private const string FormDataMimeType = "multipart/form-data";
        private static readonly string[] FormFilePropertyNames =
            typeof(IFormFile).GetTypeInfo().DeclaredProperties.Select(x => x.Name).ToArray();

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.Any(x => x.ModelMetadata.ModelType == typeof(IFormFile)))
            {
                var formFileParameterName = context
                    .ApiDescription
                    .ActionDescriptor
                    .Parameters
                    .Where(x => x.ParameterType == typeof(IFormFile))
                    .Select(x => x.Name)
                    .First();
                var parameter = new NonBodyParameter()
                {
                    Name = formFileParameterName,
                    In = "formData",
                    Description = "The file to upload.",
                    Required = true,
                    Type = "file"
                };

                var oldParameter = operation.Parameters.Single(p => p.Name == formFileParameterName);
                operation.Parameters.Remove(oldParameter);

                operation.Parameters.Add(parameter);

                if (!operation.Consumes.Contains(FormDataMimeType))
                {
                    operation.Consumes.Add(FormDataMimeType);
                }
            }
        }
    }
}
