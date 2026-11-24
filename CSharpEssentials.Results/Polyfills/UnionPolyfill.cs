#if !NET11_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Internal copy of the .NET 11 <c>UnionAttribute</c>. The C# 15 compiler finds it by name, so the typed results get union
/// pattern matching on every target framework. A public copy would clash with the runtime type on net11.0 consumers.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class UnionAttribute : Attribute;

/// <summary>
/// Internal copy of the .NET 11 <c>IUnion</c> interface.
/// </summary>
internal interface IUnion
{
    /// <summary>The value of the case the union holds, or <see langword="null"/> when it holds none.</summary>
    object? Value { get; }
}
#endif
