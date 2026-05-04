using CSharpEssentials.Enums;

namespace Examples.Enums;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("CSharpEssentials.Enums Example");
        Console.WriteLine("========================================\n");

        // ============================================================================
        // STRING ENUM ATTRIBUTE
        // ============================================================================
        Console.WriteLine("--- StringEnumAttribute ---");

        // The [StringEnum] attribute marks enums that should be serialized
        // as strings (instead of integers) in JSON or other contexts.
        Status status = Status.Active;
        Console.WriteLine($"Status.Active value: {(int)status}");
        Console.WriteLine($"Status.Active name: {status}");

        Priority priority = Priority.High;
        Console.WriteLine($"Priority.High value: {(int)priority}");
        Console.WriteLine($"Priority.High name: {priority}");

        // The library's own ErrorType enum is also marked with [StringEnum]
        Console.WriteLine("ErrorType in library is also [StringEnum]");
        Console.WriteLine();

        // ============================================================================
        // ENUM PARSING WITH STANDARD C#
        // ============================================================================
        Console.WriteLine("--- Enum Parsing ---");

        bool parsed = Enum.TryParse("Active", out Status activeValue);
        Console.WriteLine($"Parse 'Active': {parsed}, Value={activeValue}");

        bool unknown = Enum.TryParse("Unknown", out Status unknownValue);
        Console.WriteLine($"Parse 'Unknown': {unknown}");
        Console.WriteLine();

        // ============================================================================
        // SOURCE GENERATED FAST ToString & FORMATTING
        // ============================================================================
        Console.WriteLine("--- Source Generated String Formatting ---");

        Status s = Status.Active;

        string text = s.ToOptimizedString();
        Console.WriteLine($"Status.Active.ToOptimizedString() = {text}");

        string snake = s.ToSnakeCase();
        Console.WriteLine($"Status.Active.ToSnakeCase() = {snake}");

        string kebab = s.ToKebabCase();
        Console.WriteLine($"Status.Active.ToKebabCase() = {kebab}");

        string lower = s.ToLowerCase();
        Console.WriteLine($"Status.Active.ToLowerCase() = {lower}");

        string upper = s.ToUpperCase();
        Console.WriteLine($"Status.Active.ToUpperCase() = {upper}");
        Console.WriteLine();

        // ============================================================================
        // SOURCE GENERATED CONSTANTS
        // ============================================================================
        Console.WriteLine("--- Source Generated Constants ---");
        Console.WriteLine($"StatusExtensions.ActiveSnakeCase = {StatusExtensions.ActiveSnakeCase}");
        Console.WriteLine($"StatusExtensions.PendingKebabCase = {StatusExtensions.PendingKebabCase}");
        Console.WriteLine();

        // ============================================================================
        // SOURCE GENERATED LOOKUP & PARSE
        // ============================================================================
        Console.WriteLine("--- Source Generated Lookup & Parse ---");

        bool known = StatusExtensions.IsDefined("Pending");
        Console.WriteLine($"StatusExtensions.IsDefined(\"Pending\") = {known}");

        bool notKnown = StatusExtensions.IsDefined("Deleted");
        Console.WriteLine($"StatusExtensions.IsDefined(\"Deleted\") = {notKnown}");

        if (StatusExtensions.TryParse("Inactive", out Status parsedStatus))
        {
            Console.WriteLine($"StatusExtensions.TryParse(\"Inactive\") = {parsedStatus}");
        }

        string[] names = StatusExtensions.GetNames();
        Console.WriteLine($"StatusExtensions.GetNames() = [{string.Join(", ", names)}]");

        Status[] values = StatusExtensions.GetValues();
        Console.WriteLine($"StatusExtensions.GetValues() = [{string.Join(", ", values)}]");
        Console.WriteLine();

        Console.WriteLine("========================================");
        Console.WriteLine("Demo complete.");
        Console.WriteLine("Note: The StringEnumAttribute is used by");
        Console.WriteLine("      JSON converters to serialize enums");
        Console.WriteLine("      as strings instead of integers.");
        Console.WriteLine("      Source generators produce fast");
        Console.WriteLine("      ToString, parsing, and formatting.");
        Console.WriteLine("========================================");
    }
}

// ============================================================================
// ENUMS
// ============================================================================

[StringEnum]
public enum Status
{
    Active,
    Inactive,
    Pending,
    Deleted
}

[StringEnum]
public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
