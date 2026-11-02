#if NET5_0_OR_GREATER
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Runtime.CompilerServices.IsExternalInit))]
#else
namespace System.Runtime.CompilerServices;

public static class IsExternalInit { }
#endif
