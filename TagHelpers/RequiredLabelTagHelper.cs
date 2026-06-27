using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PediatriNobetSistemi.TagHelpers
{
    [HtmlTargetElement("label", Attributes = "asp-for")]
    public class RequiredLabelTagHelper : LabelTagHelper
    {
        public RequiredLabelTagHelper(IHtmlGenerator generator) : base(generator) { }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            if (For?.Metadata?.ContainerType == null) return;

            var prop = For.Metadata.ContainerType.GetProperty(For.Metadata.PropertyName ?? "");
            if (prop == null) return;

            bool isRequired = prop.GetCustomAttribute<RequiredAttribute>() != null
                              || (Nullable.GetUnderlyingType(prop.PropertyType) == null
                                  && prop.PropertyType.IsValueType);

            if (isRequired)
            {
                var existingClass = output.Attributes["class"]?.Value?.ToString() ?? "";
                output.Attributes.SetAttribute("class", (existingClass + " required").Trim());
            }
        }
    }
}
