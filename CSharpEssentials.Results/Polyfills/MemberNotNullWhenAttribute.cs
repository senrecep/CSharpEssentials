#if !NET5_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
public sealed class MemberNotNullWhenAttribute : Attribute
{
    public MemberNotNullWhenAttribute(bool returnValue, string member)
    {
        ReturnValue = returnValue;
        Member = member;
    }

    public bool ReturnValue { get; }
    public string Member { get; }
}
#endif

