using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;

namespace CSharpEssentials.AspNetCore;

[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching Result<T, E1..E3>.")]
public static partial class HttpResultExtensions
{
    /// <summary>Converts the result to an HTTP response: <c>200 OK</c> with the success value, or the problem response of the error.</summary>
    /// <param name="result">The result.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static HttpResult<Ok<T>, E1, E2, E3> ToHttpResult<T, E1, E2, E3>(this Result<T, E1, E2, E3> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        result.ToHttpResult(static value => TypedResults.Ok(value));

    /// <summary>Awaits the result, then converts the result to an HTTP response: <c>200 OK</c> with the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async Task<HttpResult<Ok<T>, E1, E2, E3>> ToHttpResult<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToHttpResult();

    /// <summary>Awaits the result, then converts the result to an HTTP response: <c>200 OK</c> with the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async ValueTask<HttpResult<Ok<T>, E1, E2, E3>> ToHttpResult<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToHttpResult();

    /// <summary>Converts the result to an HTTP response: <paramref name="onSuccess"/> for the success value, or the problem response of the error.</summary>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">Creates the response of the success value, for example <c>v =&gt; TypedResults.Accepted(uri, v)</c>.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static HttpResult<TSuccess, E1, E2, E3> ToHttpResult<T, E1, E2, E3, TSuccess>(this Result<T, E1, E2, E3> result, Func<T, TSuccess> onSuccess)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError
        where TSuccess : IResult, IEndpointMetadataProvider
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(onSuccess(result.RawValue)),
            TypedResultLogic.DefaultTag => throw TypedResultLogic.DefaultInitialized(),
            _ => new(result.CurrentError().ToProblemResult()),
        };
    }

    /// <summary>Awaits the result, then converts the result to an HTTP response: <paramref name="onSuccess"/> for the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <param name="onSuccess">Creates the response of the success value, for example <c>v =&gt; TypedResults.Accepted(uri, v)</c>.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async Task<HttpResult<TSuccess, E1, E2, E3>> ToHttpResult<T, E1, E2, E3, TSuccess>(this Task<Result<T, E1, E2, E3>> result, Func<T, TSuccess> onSuccess)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError
        where TSuccess : IResult, IEndpointMetadataProvider =>
        (await result.ConfigureAwait(false)).ToHttpResult(onSuccess);

    /// <summary>Awaits the result, then converts the result to an HTTP response: <paramref name="onSuccess"/> for the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <param name="onSuccess">Creates the response of the success value, for example <c>v =&gt; TypedResults.Accepted(uri, v)</c>.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async ValueTask<HttpResult<TSuccess, E1, E2, E3>> ToHttpResult<T, E1, E2, E3, TSuccess>(this ValueTask<Result<T, E1, E2, E3>> result, Func<T, TSuccess> onSuccess)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError
        where TSuccess : IResult, IEndpointMetadataProvider =>
        (await result.ConfigureAwait(false)).ToHttpResult(onSuccess);

    /// <summary>Converts the result to <c>200 OK</c> with the success value, or the problem response of the error.</summary>
    /// <param name="result">The result.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static HttpResult<Ok<T>, E1, E2, E3> ToOk<T, E1, E2, E3>(this Result<T, E1, E2, E3> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        result.ToHttpResult();

    /// <summary>Awaits the result, then converts the result to <c>200 OK</c> with the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async Task<HttpResult<Ok<T>, E1, E2, E3>> ToOk<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToOk();

    /// <summary>Awaits the result, then converts the result to <c>200 OK</c> with the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async ValueTask<HttpResult<Ok<T>, E1, E2, E3>> ToOk<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToOk();

    /// <summary>Converts the result to <c>201 Created</c> with the success value and its location, or the problem response of the error.</summary>
    /// <param name="result">The result.</param>
    /// <param name="location">Returns the <c>Location</c> header of the created value.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static HttpResult<Created<T>, E1, E2, E3> ToCreated<T, E1, E2, E3>(this Result<T, E1, E2, E3> result, Func<T, string?> location)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError
    {
        ArgumentNullException.ThrowIfNull(location);
        return result.ToHttpResult(value => TypedResults.Created(location(value), value));
    }

    /// <summary>Awaits the result, then converts the result to <c>201 Created</c> with the success value and its location, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <param name="location">Returns the <c>Location</c> header of the created value.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async Task<HttpResult<Created<T>, E1, E2, E3>> ToCreated<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> result, Func<T, string?> location)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToCreated(location);

    /// <summary>Awaits the result, then converts the result to <c>201 Created</c> with the success value and its location, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <param name="location">Returns the <c>Location</c> header of the created value.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async ValueTask<HttpResult<Created<T>, E1, E2, E3>> ToCreated<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> result, Func<T, string?> location)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToCreated(location);

    /// <summary>Converts the result to <c>204 No Content</c>, dropping the success value, or the problem response of the error.</summary>
    /// <param name="result">The result.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static HttpResult<NoContent, E1, E2, E3> ToNoContent<T, E1, E2, E3>(this Result<T, E1, E2, E3> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        result.ToHttpResult(static _ => TypedResults.NoContent());

    /// <summary>Awaits the result, then converts the result to <c>204 No Content</c>, dropping the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async Task<HttpResult<NoContent, E1, E2, E3>> ToNoContent<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToNoContent();

    /// <summary>Awaits the result, then converts the result to <c>204 No Content</c>, dropping the success value, or the problem response of the error.</summary>
    /// <param name="result">The result to await.</param>
    /// <returns>The HTTP response.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public static async ValueTask<HttpResult<NoContent, E1, E2, E3>> ToNoContent<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> result)
        where T : notnull
        where E1 : IError
        where E2 : IError
        where E3 : IError =>
        (await result.ConfigureAwait(false)).ToNoContent();
}
