using System;
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.Entity;

public static class Extensions
{
    public static void HardDelete<T>(this IEnumerable<T> entities) where T : ISoftDeletable
    {
        foreach (T entity in entities)
            entity.MarkAsHardDeleted();
    }
}
