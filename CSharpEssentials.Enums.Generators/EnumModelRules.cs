namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Member rules shared by the analyzer (which reports them) and the generator (which skips enums with errors).
/// Names are compared case-insensitively because data reads match wire names, member names and aliases ignoring case.
/// </summary>
internal static class EnumModelRules
{
    public static bool HasErrors(EnumModel model, GeneratorSettings settings)
    {
        foreach (EnumRuleViolation violation in Check(model.Members, StringEnumSourceWriter.WireNames(model, settings), model.IsFlags))
        {
            if (violation.IsError)
                return true;
        }

        return false;
    }

    public static List<EnumRuleViolation> Check(IReadOnlyList<EnumMemberModel> members, string[] wireNames, bool isFlags)
    {
        List<EnumRuleViolation> violations = [];
        CheckWireNames(members, wireNames, violations);
        CheckAliases(members, wireNames, violations);
        CheckFallbacks(members, isFlags, violations);
        if (isFlags)
            CheckFlags(members, violations);
        return violations;
    }

    /// <summary>The reason a wire name or alias cannot be read back, or <see langword="null"/> when it is valid.</summary>
    public static string? InvalidReason(string name)
    {
        if (name.Length == 0)
            return "it is empty";
        foreach (char c in name)
        {
            if (char.IsWhiteSpace(c))
                return "it contains whitespace";
            if (c == ',')
                return "it contains a comma, which separates flags";
        }

        // Matches EnumNumberParser.LooksNumeric: such text is always read as a number.
        char first = name[0];
        return first is >= '0' and <= '9' or '-' or '+' or '.'
            ? "it starts with a digit, a sign or a dot, so it is read as a number"
            : null;
    }

    private static void CheckWireNames(IReadOnlyList<EnumMemberModel> members, string[] wireNames, List<EnumRuleViolation> violations)
    {
        Dictionary<string, int> seen = [with(StringComparer.OrdinalIgnoreCase)];
        for (int i = 0; i < members.Count; i++)
        {
            string? reason = InvalidReason(wireNames[i]);
            if (reason is not null)
                violations.Add(new EnumRuleViolation(EnumRuleKind.InvalidWireName, i, -1, wireNames[i], reason));

            if (seen.TryGetValue(wireNames[i], out int first))
                violations.Add(new EnumRuleViolation(EnumRuleKind.WireNameCollision, i, -1, wireNames[i], members[first].Name));
            else
                seen.Add(wireNames[i], i);
        }
    }

    private static void CheckAliases(IReadOnlyList<EnumMemberModel> members, string[] wireNames, List<EnumRuleViolation> violations)
    {
        for (int i = 0; i < members.Count; i++)
        {
            EquatableArray<string> aliases = members[i].Aliases;
            for (int a = 0; a < aliases.Count; a++)
            {
                string alias = aliases[a];
                string? reason = InvalidReason(alias);
                if (reason is not null)
                    violations.Add(new EnumRuleViolation(EnumRuleKind.InvalidWireName, i, a, alias, reason));

                string? collision = FindCollision(members, wireNames, i, alias);
                if (collision is not null)
                    violations.Add(new EnumRuleViolation(EnumRuleKind.AliasCollision, i, a, alias, collision));
            }
        }
    }

    private static string? FindCollision(IReadOnlyList<EnumMemberModel> members, string[] wireNames, int owner, string alias)
    {
        for (int j = 0; j < members.Count; j++)
        {
            if (j == owner)
                continue;

            EnumMemberModel other = members[j];
            if (string.Equals(wireNames[j], alias, StringComparison.OrdinalIgnoreCase))
                return "the wire name of '" + other.Name + "'";
            if (string.Equals(other.Name, alias, StringComparison.OrdinalIgnoreCase))
                return "the member name '" + other.Name + "'";
            foreach (string otherAlias in other.Aliases)
            {
                if (string.Equals(otherAlias, alias, StringComparison.OrdinalIgnoreCase))
                    return "an alias of '" + other.Name + "'";
            }
        }

        return null;
    }

    private static void CheckFallbacks(IReadOnlyList<EnumMemberModel> members, bool isFlags, List<EnumRuleViolation> violations)
    {
        List<int> fallbacks = [];
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].IsFallback)
                fallbacks.Add(i);
        }

        foreach (int index in fallbacks)
        {
            if (isFlags)
                violations.Add(new EnumRuleViolation(EnumRuleKind.FallbackOnFlags, index, -1, members[index].Name, string.Empty));
            if (fallbacks.Count > 1)
            {
                string others = string.Join("', '", fallbacks.Where(other => other != index).Select(other => members[other].Name));
                violations.Add(new EnumRuleViolation(EnumRuleKind.MultipleFallbacks, index, -1, members[index].Name, others));
            }
        }
    }

    private static void CheckFlags(IReadOnlyList<EnumMemberModel> members, List<EnumRuleViolation> violations)
    {
        bool hasZero = false;
        for (int i = 0; i < members.Count; i++)
        {
            ulong value = members[i].RawValue;
            if (value == 0)
            {
                hasZero = true;
                continue;
            }

            if ((value & (value - 1)) != 0 && CoveredBits(members, i) != value)
                violations.Add(new EnumRuleViolation(EnumRuleKind.InvalidFlagValue, i, -1, members[i].Name, string.Empty));
        }

        if (!hasZero)
            violations.Add(new EnumRuleViolation(EnumRuleKind.FlagsWithoutZero, -1, -1, string.Empty, string.Empty));
    }

    private static ulong CoveredBits(IReadOnlyList<EnumMemberModel> members, int index)
    {
        ulong value = members[index].RawValue;
        ulong covered = 0;
        for (int j = 0; j < members.Count; j++)
        {
            ulong other = members[j].RawValue;
            if (other != 0 && other != value && (other & ~value) == 0)
                covered |= other;
        }

        return covered;
    }
}
