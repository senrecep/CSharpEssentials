using System.Text.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Tests.Results;

internal static class NullSuccessResult
{
    public static Result<T> Of<T>() => JsonSerializer.Deserialize<Result<T>>("{\"IsSuccess\":true,\"Value\":null}");
}
