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

        // AsSpan avoids all intermediate string allocations.
        // Trim() on ReadOnlySpan<char> is available on netstandard2.1+.
        ReadOnlySpan<char> afterArrow = memberExpr.AsSpan(arrowIdx + 2).Trim();

        int dotIdx = FindFirstOuterDot(afterArrow);
        if (dotIdx < 0)
        {
            // No dot: expression targets the variable itself (e.g. "() => item").
            // TrimEnd('!') handles trailing null-forgiving operator (e.g. "() => item!").
            return (afterArrow.TrimEnd('!').ToString(), true);
        }

        return (Normalize(afterArrow[(dotIdx + 1)..]), false);
    }

    private static string Normalize(ReadOnlySpan<char> path)
    {
        if (path.IndexOfAny('(', '!', '?') < 0)
            return path.ToString();

        System.Text.StringBuilder builder = new(path.Length);
        int segmentStart = 0;
        int depth = 0;
        for (int i = 0; i <= path.Length; i++)
        {
            bool atEnd = i == path.Length;
            char c = atEnd ? '.' : path[i];
            if (!atEnd)
            {
                if (c == '(' && depth == 0 && IsCall(path, segmentStart, i))
                    break;
                if (c is '(' or '[')
                    depth++;
                else if (c is ')' or ']')
                    depth--;
            }

            if (c == '.' && depth == 0)
            {
                AppendSegment(builder, path[segmentStart..i]);
                segmentStart = i + 1;
            }
        }

        if (builder.Length == 0)
            return path.TrimEnd('!').ToString();

        return builder.ToString();
    }

    private static bool IsCall(ReadOnlySpan<char> path, int segmentStart, int parenIndex) =>
        parenIndex > segmentStart && (char.IsLetterOrDigit(path[parenIndex - 1]) || path[parenIndex - 1] is '_' or '>');

    private static void AppendSegment(System.Text.StringBuilder builder, ReadOnlySpan<char> segment)
    {
        segment = segment.Trim().TrimEnd('!').TrimEnd('?');
        if (segment.IsEmpty)
            return;
        if (builder.Length > 0)
            builder.Append('.');
        builder.Append(segment);
    }

    /// <summary>
    /// Returns the index of the first <c>.</c> not inside parentheses.
    /// Handles cast expressions such as <c>((IFoo)model.Child).Name</c> —
    /// the dot inside the cast is at depth 1 and is skipped; the outer dot is returned.
    /// Null-conditional (<c>?.</c>) and null-forgiving (<c>!.</c>) prefixes are handled
    /// naturally: <c>?</c> and <c>!</c> are neither <c>(</c> nor <c>)</c> so depth stays
    /// unchanged; the following <c>.</c> is found at the correct depth.
    /// </summary>
    private static int FindFirstOuterDot(ReadOnlySpan<char> expr)
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
