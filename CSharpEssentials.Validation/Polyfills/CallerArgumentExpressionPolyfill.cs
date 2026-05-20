#if NETSTANDARD2_1
// CallerArgumentExpression is a C# 10 compiler feature.
// The compiler recognises the attribute regardless of target framework —
// we only need to define it when the BCL does not already include it.
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}
#endif
