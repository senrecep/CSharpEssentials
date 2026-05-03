using CSharpEssentials.Enums;

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

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("Note: The StringEnumAttribute is used by");
Console.WriteLine("      JSON converters to serialize enums");
Console.WriteLine("      as strings instead of integers.");
Console.WriteLine("========================================");

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
