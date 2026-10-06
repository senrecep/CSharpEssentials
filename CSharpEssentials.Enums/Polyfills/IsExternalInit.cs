#if NET5_0_OR_GREATER
// An init accessor carries modreq(IsExternalInit). Code compiled against the netstandard assets (a netstandard2.0 contracts
// assembly with generated enum metadata) references this assembly's polyfill; the forwarder makes that reference resolve to the
// runtime type, so its init accessor calls bind to the net assets at run time instead of throwing MissingMethodException.
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Runtime.CompilerServices.IsExternalInit))]
#else
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit;
#endif
