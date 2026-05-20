namespace CSharpEssentials.Validation;

/// <summary>
/// Extracts a human-readable property path from a lambda captured by
/// <see cref="System.Runtime.CompilerServices.CallerArgumentExpressionAttribute"/>.
/// </summary>
/// <example>
/// <c>"() => model.Name"</c>          → <c>"Name"</c><br/>
/// <c>"() => model.Address.City"</c>  → <c>"Address.City"</c>
/// </example>
internal static class PropertyPath
{
    /// <summary>
    /// Extracts the property path from a CallerArgumentExpression string such as
    /// <c>"() => model.Name"</c> or <c>"() => model.Address.City"</c>.
    /// </summary>
    internal static string Extract(string memberExpr) => ExtractWithMeta(memberExpr).PropertyName;

    /// <summary>
    /// Like <see cref="Extract"/> but also indicates whether the expression targets
    /// the captured variable itself (no property access) vs. a nested property.
    /// Used by <see cref="RuleContext{T}"/> when building prefixed names inside
    /// <c>ForEach</c> to distinguish <c>"() => item"</c> from <c>"() => item.Name"</c>.
    /// </summary>
    /// <returns>
    /// <c>(propertyName, isItemSelf)</c> where <c>isItemSelf</c> is <see langword="true"/>
    /// when there is no dot after the variable (e.g. <c>"() => item"</c>).
    /// </returns>
    internal static (string PropertyName, bool IsItemSelf) ExtractWithMeta(string memberExpr)
    {
        if (string.IsNullOrWhiteSpace(memberExpr))
            return (memberExpr, true);

        int arrowIdx = memberExpr.IndexOf("=>", StringComparison.Ordinal);
        if (arrowIdx < 0)
            return (memberExpr.Trim(), true);

        string afterArrow = memberExpr[(arrowIdx + 2)..].Trim();

        // Normalize null-conditional and null-forgiving operators: ?. → . and !. → .
        // Then strip any remaining standalone ! (e.g. trailing null-forgiving: "model.Name!")
        afterArrow = afterArrow.Replace("?.", ".", StringComparison.Ordinal)
                               .Replace("!.", ".", StringComparison.Ordinal)
                               .Replace("!", "", StringComparison.Ordinal);

        int dotIdx = FindFirstOuterDot(afterArrow);
        if (dotIdx < 0)
            return (afterArrow, true);

        return (afterArrow[(dotIdx + 1)..], false);
    }

    /// <summary>
    /// Returns the index of the first <c>.</c> not inside parentheses.
    /// Handles cast expressions such as <c>((IFoo)model.Child).Name</c> —
    /// the dot inside the cast is at depth 1 and is skipped; the outer dot is returned.
    /// </summary>
    private static int FindFirstOuterDot(string expr)
    {
        int depth = 0;
        for (int i = 0; i < expr.Length; i++)
        {
            char c = expr[i];
            if (c == '(')
                depth++;
            else if (c == ')')
                depth--;
            else if (c == '.' && depth == 0)
                return i;
        }
        return -1;
    }
}
