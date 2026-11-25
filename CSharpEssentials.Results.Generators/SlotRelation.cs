namespace CSharpEssentials.Results.Generators;

/// <summary>
/// How a later error slot of a typed result relates to an earlier one.
/// </summary>
internal enum SlotRelation
{
    Repeats,
    ConvertsTo,
    Accepts,
}
