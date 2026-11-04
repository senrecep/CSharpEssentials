using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace CSharpEssentials.EntityFrameworkCore;

internal sealed class OrderedGuidValueGenerator(Func<Guid> next) : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry) => next();
}
