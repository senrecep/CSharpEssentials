using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

namespace AotProbe;

internal sealed class Order : SoftDeletableEntityBase;

internal sealed record OrderPlaced : IDomainEvent;
