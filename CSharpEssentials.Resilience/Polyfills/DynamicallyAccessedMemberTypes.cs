#if !NET5_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[Flags]
internal enum DynamicallyAccessedMemberTypes
{
    None = 0,
    All = ~None,
}
#endif
