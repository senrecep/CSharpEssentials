using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A <see cref="ProblemDetails"/> built from <see cref="Error"/> values.
/// Which error collections are serialized is controlled by <see cref="EnhancedProblemDetailsOptions.ErrorFields"/>.
/// Every serialized field lives in <see cref="ProblemDetails.Extensions"/>, so the JSON is the same whether a writer
/// serializes the runtime type or the declared <see cref="ProblemDetails"/> type (as MVC's API problem writer does).
/// </summary>
public sealed class EnhancedProblemDetails : ProblemDetails
{
    internal const string ErrorCodesKey = "errorCodes";
    internal const string ErrorMessagesKey = "errorMessages";

    /// <summary>
    /// The errors the problem was built from. Not serialized directly; see <see cref="ProblemErrorFields"/>.
    /// </summary>
    [JsonIgnore]
    public Error[] Errors { get; set; } = [];

    /// <summary>
    /// Distinct error codes, written as the <c>errorCodes</c> extension when <see cref="ProblemErrorFields.Codes"/> is enabled.
    /// </summary>
    [JsonIgnore]
    public HashSet<string>? ErrorCodes
    {
        get => GetSet(ErrorCodesKey);
        set => SetSet(ErrorCodesKey, value);
    }

    /// <summary>
    /// Distinct error descriptions, written as the <c>errorMessages</c> extension when <see cref="ProblemErrorFields.Messages"/> is enabled.
    /// </summary>
    [JsonIgnore]
    public HashSet<string>? ErrorMessages
    {
        get => GetSet(ErrorMessagesKey);
        set => SetSet(ErrorMessagesKey, value);
    }

    private HashSet<string>? GetSet(string key) =>
        Extensions.TryGetValue(key, out object? value) ? value as HashSet<string> : null;

    private void SetSet(string key, HashSet<string>? value)
    {
        if (value is null)
            Extensions.Remove(key);
        else
            Extensions[key] = value;
    }
}
