using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// A later error slot, by its index among the error slots, that repeats, converts to or accepts an earlier slot.
/// </summary>
internal readonly record struct SlotPair(int LaterIndex, ITypeSymbol Later, ITypeSymbol Earlier, SlotRelation Relation);
