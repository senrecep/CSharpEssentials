using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

internal sealed class StoredOrderModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) =>
        (context.GetType(), ((StoredOrderContext)context).Setup.Key, designTime);
}
