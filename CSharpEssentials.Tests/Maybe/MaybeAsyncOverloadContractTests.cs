using System.Reflection;
using System.Text.RegularExpressions;
using CSharpEssentials.Maybe;
using FluentAssertions;
using MaybeFactory = CSharpEssentials.Maybe.Maybe;

namespace CSharpEssentials.Tests.Maybe;

/// <summary>Reflection guards for the Maybe async naming: every obsolete forwarder has an <c>*Async</c> twin, and the overload priority sits on the right members.</summary>
public sealed class MaybeAsyncOverloadContractTests
{
    private const BindingFlags AllPublic = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly string[] PrioritisedNames = ["ExecuteAsync", "ExecuteNoValueAsync", "OrAsync", "MatchAsync"];

    private static readonly Type[] MaybeTypes = [typeof(Maybe<>), typeof(MaybeExtensions), typeof(MaybeFactory)];

    private static string Signature(MethodInfo method, string name) =>
        $"{method.ReturnType}|{name}|{method.GetGenericArguments().Length}|{string.Join(",", method.GetParameters().Select(p => p.ParameterType.ToString()))}";

    private static bool HasPriority(MethodInfo method) =>
        method.GetCustomAttributes().Any(a => a.GetType().Name == "OverloadResolutionPriorityAttribute");

    public static TheoryData<string> ObsoleteForwarders()
    {
        var data = new TheoryData<string>();
        foreach (MethodInfo method in MaybeTypes.SelectMany(t => t.GetMethods(AllPublic)))
        {
            if (method.GetCustomAttribute<ObsoleteAttribute>() is not null && method.DeclaringType is { } type)
                data.Add($"{type.Name}.{Signature(method, method.Name)}");
        }
        return data;
    }

    [Fact]
    public void ObsoleteForwarders_Should_BeFound_When_ScanningMaybeTypes()
    {
        ObsoleteForwarders().Should().HaveCountGreaterThanOrEqualTo(60);
    }

    [Fact]
    public void ObsoleteForwarder_Should_HaveAsyncTwinWithSameSignature_When_MarkedObsolete()
    {
        List<string> missing = [];
        foreach (Type type in MaybeTypes)
        {
            MethodInfo[] methods = type.GetMethods(AllPublic);
            HashSet<string> asyncSignatures = methods
                .Where(m => m.Name.EndsWith("Async", StringComparison.Ordinal))
                .Select(m => Signature(m, m.Name))
                .ToHashSet();

            foreach (MethodInfo forwarder in methods.Where(m => m.GetCustomAttribute<ObsoleteAttribute>() is not null))
            {
                string expected = Signature(forwarder, forwarder.Name + "Async");
                if (!asyncSignatures.Contains(expected))
                    missing.Add($"{type.Name}.{forwarder.Name}: {expected}");
            }
        }

        missing.Should().BeEmpty();
    }

    [Fact]
    public void ObsoleteForwarder_Should_NameItsReplacement_When_MarkedObsolete()
    {
        List<string> wrong = [];
        foreach (MethodInfo forwarder in MaybeTypes.SelectMany(t => t.GetMethods(AllPublic)))
        {
            ObsoleteAttribute? obsolete = forwarder.GetCustomAttribute<ObsoleteAttribute>();
            if (obsolete is null)
                continue;

            string expected = $"Use {forwarder.Name}Async. Will be removed in 7.0.";
            if (obsolete.Message != expected)
                wrong.Add($"{forwarder.DeclaringType!.Name}.{forwarder.Name}: {obsolete.Message}");
        }

        wrong.Should().BeEmpty();
    }

    [Fact]
    public void OverloadPriority_Should_SitOnTaskOverloadsOnly_When_MemberHasValueTaskTwin()
    {
        List<string> wrong = [];
        IEnumerable<MethodInfo> instanceMethods = typeof(Maybe<>).GetMethods(AllPublic).Where(m => !m.IsStatic);
        IEnumerable<MethodInfo> keyValueMatches = typeof(MaybeExtensions).GetMethods(AllPublic)
            .Where(m => m.Name == "MatchAsync" && m.GetParameters()[0].ParameterType.ToString().Contains("KeyValuePair", StringComparison.Ordinal));

        foreach (MethodInfo method in instanceMethods.Concat(keyValueMatches).Where(m => PrioritisedNames.Contains(m.Name)))
        {
            bool mentionsValueTask = Regex.IsMatch(method.ReturnType + string.Join(",", method.GetParameters().Select(p => p.ParameterType)), "ValueTask");
            if (HasPriority(method) == mentionsValueTask)
                wrong.Add($"{method.DeclaringType!.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) priority={HasPriority(method)}");
        }

        wrong.Should().BeEmpty();
    }

    [Fact]
    public void AsyncMembers_Should_EndInAsync_When_ReturningTaskOrValueTask()
    {
        List<string> unsuffixed = [];
        foreach (MethodInfo method in MaybeTypes.SelectMany(t => t.GetMethods(AllPublic)))
        {
            bool returnsTask = method.ReturnType == typeof(Task) || method.ReturnType == typeof(ValueTask)
                || (method.ReturnType.IsGenericType && (method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) || method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)));
            if (returnsTask && !method.Name.EndsWith("Async", StringComparison.Ordinal) && method.GetCustomAttribute<ObsoleteAttribute>() is null)
                unsuffixed.Add($"{method.DeclaringType!.Name}.{method.Name}");
        }

        unsuffixed.Should().BeEmpty();
    }
}
