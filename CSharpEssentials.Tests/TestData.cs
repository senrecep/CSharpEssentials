using CSharpEssentials.Errors;

namespace CSharpEssentials.Tests;

internal static class TestData
{
    internal static class Strings
    {
        public static readonly string Empty = string.Empty;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
        public static readonly string? Null;
#pragma warning restore CS0649
        public const string Whitespace = "   \t\n\r  ";
        public const string SingleChar = "A";
        public const string PascalCase = "HelloWorld";
        public const string CamelCase = "helloWorld";
        public const string KebabCase = "hello-world";
        public const string SnakeCase = "hello_world";
        public const string MacroCase = "HELLO_WORLD";
        public const string TitleCase = "Hello World";
        public const string TrainCase = "Hello-World";
        public const string UnderscoreCamelCase = "_helloWorld";
        public const string WithNumbers = "Hello123World456";
        public const string WithSpecialChars = "Hello@World#Test";
        public const string Unicode = "Hello世界مرحبا";
        public static readonly string LongString = new('A', 10000);
        public const string MixedCase = "hELLo WoRLd";
    }

    internal static class Collections
    {
        public static int[] EmptyIntArray => Array.Empty<int>();
        public static List<int> EmptyIntList => [];
        public static int[] IntArray => [1, 2, 3, 4, 5];
        public static List<int> IntList => [1, 2, 3, 4, 5];
        public static string[] StringArray => ["a", "b", "c", "d", "e"];
        public static List<string> StringList => ["a", "b", "c", "d", "e"];
        public static string?[] NullableStringArray => ["a", null, "c", null, "e"];
        public static List<string?> NullableStringList => ["a", null, "c", null, "e"];
        public static bool[] BoolArray => [true, false, true, false, true];
        public static List<bool> BoolList => [true, false, true, false, true];
    }

    internal static class Guids
    {
        public static Guid ValidGuid { get; } = Guid.NewGuid();
        public static readonly Guid EmptyGuid = Guid.Empty;
        public static string ValidGuidBase64 => Convert.ToBase64String(ValidGuid.ToByteArray())
            .TrimEnd('=').Replace("/", "_").Replace("+", "-");
    }

    internal static class Errors
    {
        public static readonly Error Failure = Error.Failure("TEST.FAILURE", "Test failure message");
        public static readonly Error Validation = Error.Validation("TEST.VALIDATION", "Test validation message");
        public static readonly Error NotFound = Error.NotFound("TEST.NOT_FOUND", "Test not found message");
        public static readonly Error Conflict = Error.Conflict("TEST.CONFLICT", "Test conflict message");
        public static readonly Error Unauthorized = Error.Unauthorized("TEST.UNAUTHORIZED", "Test unauthorized message");
        public static readonly Error Forbidden = Error.Forbidden("TEST.FORBIDDEN", "Test forbidden message");
        public static readonly Error Unexpected = Error.Unexpected("TEST.UNEXPECTED", "Test unexpected message");

        public static ErrorMetadata SimpleMetadata => new("key1", "value1");
        public static ErrorMetadata ComplexMetadata => new()
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true }
        };
    }

    internal static class Dates
    {
        public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        public static readonly DateTimeOffset PastDate = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset FutureDate = new(2030, 12, 31, 23, 59, 59, TimeSpan.Zero);
        public const long UnixTimestampMs = 1609459200000; // 2021-01-01 00:00:00 UTC
    }
}
