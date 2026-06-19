using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BricklePlatform.Api.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromHeaderModelAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
{
    public BindingSource BindingSource => BindingSource.Query;
    public string Name { get; set; } = string.Empty;
}