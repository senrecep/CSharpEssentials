using System.Net.Http.Json;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpContentExtensions
{
    public static async Task<Result<string>> ReadAsStringAsResultAsync(this HttpContent content, CancellationToken cancellationToken = default)
    {
        try
        {
#if NETSTANDARD2_1
            string value = await content.ReadAsStringAsync();
#else
            string value = await content.ReadAsStringAsync(cancellationToken);
#endif
            return value;
        }
        catch (OperationCanceledException oce) when (oce.CancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);
        }
        catch (JsonException ex)
        {
            return Error.Exception(ex, ErrorType.Validation);
        }
        catch (Exception ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
    }

    public static async Task<Result<T>> ReadFromJsonAsResultAsync<T>(
        this HttpContent content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            T? value = await content.ReadFromJsonAsync<T>(options ?? EnhancedJsonSerializerOptions.DefaultOptions, cancellationToken);
            if (value is null)
                return Error.NotFound(description: "Response body was empty or could not be deserialized.");
            return value;
        }
        catch (OperationCanceledException oce) when (oce.CancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);
        }
        catch (JsonException ex)
        {
            return Error.Exception(ex, ErrorType.Validation);
        }
        catch (Exception ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
    }
}
