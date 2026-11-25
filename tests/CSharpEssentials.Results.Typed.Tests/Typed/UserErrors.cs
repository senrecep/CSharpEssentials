using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// An error catalog in the recommended shape: predefined static instances, a parallel <see cref="Codes"/> class for matching
/// by code, and a nested derived type for the error that needs its own case.
/// </summary>
internal static class UserErrors
{
    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.", new ErrorMetadata("reason", "gdpr"));

    [SuppressMessage("Major Code Smell", "S3218:Inner class members should not shadow outer class static or type members", Justification = "The catalog shape under test: each code constant shares the name of its predefined error.")]
    internal static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
        public const string Invalid = "User.Invalid";
    }

    internal sealed record Locked : ConflictError
    {
        public Locked(int id)
            : base(Codes.Locked, "User is locked.") => Id = id;

        public int Id { get; }
    }
}
