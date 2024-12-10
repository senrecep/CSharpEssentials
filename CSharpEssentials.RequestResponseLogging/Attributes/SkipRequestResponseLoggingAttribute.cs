namespace CSharpEssentials.RequestResponseLogging;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SkipRequestResponseLoggingAttribute : Attribute;
