namespace Hybrid.Common;

[AttributeUsage(AttributeTargets.Class)]
public class SharedHostObjectAttribute<TDefinition, TSelf> : Attribute where TSelf : class, TDefinition;