using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Results.Generators;

internal static class TypedResultDiagnostics
{
    public const string Category = "Usage";

    private const string HelpLink = "https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#diagnostics";

    public static readonly DiagnosticDescriptor DefaultResult = new(
        "CSE5001",
        "A typed result is created as default",
        "{0} creates a default '{1}', which holds no case",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A default typed result is neither a success nor an error: a switch expression over it throws " +
            "SwitchExpressionException and every combinator throws InvalidOperationException. Create it from a value or an error. " +
            "Give a field or an auto-property an initializer, assign it in a constructor or mark it required.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor DuplicateSlot = new(
        "CSE5002",
        "A typed result repeats an error type",
        "'{0}' has the error type '{1}' in two slots",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Every conversion from the repeated type is ambiguous (CS0457) and the slots cannot be told apart by a pattern. " +
            "Keep each error type once.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor OverlappingSlots = new(
        "CSE5003",
        "One error slot of a typed result fits another",
        "In '{0}' the error slot '{1}' also converts to the slot '{2}', so the static type of an error picks its slot",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An error whose static type is the base goes to the base slot even when its runtime type is the derived one, " +
            "and a pattern on the derived type does not match it. Use error types that do not derive from each other.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor SuccessFitsErrorSlot = new(
        "CSE5004",
        "The success type of a typed result overlaps an error slot",
        "In '{0}' the success type '{1}' and the error slot '{2}' convert to one another, so the static type decides between success and failure",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A value whose static type is the wider of the two takes that case whatever its runtime type is. " +
            "Use a success type that is not an error type and is not a base of one.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor ErrorKindMismatch = new(
        "CSE5005",
        "[ErrorKind] disagrees with the kind of the base error",
        "'{0}' declares [ErrorKind({1})] but derives from '{2}' of kind {3}",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The Type of a typed error kind is fixed by its base, so the attribute that tools read (status codes, OpenAPI) " +
            "and the runtime error disagree. Remove the attribute or derive from the kind it names.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor CombinatorRepeatsSlot = new(
        "CSE5006",
        "A combinator adds an error slot the result already covers",
        "'{0}' returns '{1}', where the added slot '{2}' {3} the slot '{4}'; {5}",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ensure<F> and FailIf<F> add a slot when the static type of the error is not exactly a slot type, such as a " +
            "derived error. BindWiden and the adding SelectMany add the inner result's slots even when the outer result has them, and TryCatch adds the exception type. Cast the error to the slot " +
            "type, or call Bind<TOut> and let the inner result widen.",
        helpLinkUri: HelpLink);
}
