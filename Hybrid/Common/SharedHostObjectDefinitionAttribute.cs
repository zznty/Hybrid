namespace Hybrid.Common;

[AttributeUsage(AttributeTargets.Interface)]
public class SharedHostObjectDefinitionAttribute : Attribute
{
    public bool EmitDispatchInformation { get; set; } = true;
}