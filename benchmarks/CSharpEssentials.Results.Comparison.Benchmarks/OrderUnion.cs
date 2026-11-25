#if NET11_0_OR_GREATER
namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// The C# 15 native union with the cases of <see cref="Typed"/>. The runtime stores the case as <see cref="object"/>, so
/// the <see cref="int"/> success case is boxed when the union is built.
/// </summary>
// https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union
public union OrderUnion(int, NotFoundError, ConflictError, ValidationError);
#endif
