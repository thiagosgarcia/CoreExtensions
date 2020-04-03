using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public class SwaggerFileUploadFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;

            var result = from a in context.ApiDescription.ParameterDescriptions
                join b in operation.Parameters.OfType<NonBodyParameter>()
                    on a.Name equals b?.Name
                where a?.ModelMetadata?.ModelType == typeof(IFormFile)
                select b;


            result.ToList().ForEach(x =>
            {
                x.In = "formData";
                x.Description = "Upload de arquivo";
                x.Type = "file";
            });
        }
    }

    public class AttachControllerNameFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
                operation.Summary = $"{descriptor.ControllerTypeInfo.Namespace}.{descriptor.ControllerName}Controller.{descriptor.ActionName} - {operation.Summary}";
        }
    }
}
