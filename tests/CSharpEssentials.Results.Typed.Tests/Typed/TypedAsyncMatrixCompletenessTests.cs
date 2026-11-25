using System.Reflection;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// Guards the async matrix of the typed results. The expected cells come from the synchronous combinators found by reflection and
/// from the cell table below, which copies the async matrix of <c>Result&lt;TValue&gt;</c> for each family. It does not read the
/// emitter, so a cell the emitter drops fails here. A Task cell with a ValueTask twin on the same receiver needs
/// <c>[OverloadResolutionPriority(1)]</c>. On <c>ResultExtensions</c> the same-slot Ensure and FailIf cells get 2 more than that, because
/// they and the slot-adding cells are all generic and an error of a slot type binds both; the per-slot TapError cells get 2 more so
/// that they stay above the "any error" cells. The net9.0-only cells are missing from the netstandard2.1 asset. The "any error"
/// members (constrained to <c>IError</c> slots), GetValueOrDefaultAsync and GetValueOrAsync have no handler cell table and are
/// covered by the emitted tests instead.
/// </summary>
public sealed class TypedAsyncMatrixCompletenessTests
{
    private const string PriorityAttribute = "System.Runtime.CompilerServices.OverloadResolutionPriorityAttribute";

    private sealed record Cell(string Receiver, string Handler, bool Net9Only, bool Prioritized);

    private static readonly Dictionary<string, Cell[]> Cells = new(StringComparer.Ordinal)
    {
        ["Bind"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", false, false),
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "ValueTask", false, false),
        ],
        ["Map"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", true, false),
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "ValueTask", false, false),
        ],
        ["Ensure"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", true, false),
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "Task", false, true), new("ValueTask", "ValueTask", true, false),
        ],
        ["MapError"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", false, false),
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "ValueTask", false, false),
        ],
        ["Compensate"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", true, false),
            new("Task", "Task", false, false),
            new("ValueTask", "Task", false, true), new("ValueTask", "ValueTask", true, false),
        ],
        ["Tap"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", true, false),
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "ValueTask", false, false),
        ],
        ["TapIf"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", true, false),
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "Task", false, true), new("ValueTask", "ValueTask", true, false),
        ],
        ["TapError"] =
        [
            new("this", "Task", false, true), new("this", "ValueTask", true, false),
            new("Task", "Task", false, false),
            new("ValueTask", "Task", false, true), new("ValueTask", "ValueTask", true, false),
        ],
        ["TryCatch"] = [new("Task", "sync", false, false), new("ValueTask", "sync", false, false)],
        ["Finally"] =
        [
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "ValueTask", true, false),
        ],
        ["FinallyAction"] =
        [
            new("Task", "sync", false, false), new("Task", "Task", false, false),
            new("ValueTask", "sync", false, false), new("ValueTask", "Task", false, true), new("ValueTask", "ValueTask", true, false),
        ],
        ["Select"] = [new("Task", "sync", false, false), new("Task", "Task", false, false), new("ValueTask", "sync", false, false)],
    };

    private static readonly HashSet<string> WithoutCellTable = new(StringComparer.Ordinal) { "GetValueOrDefaultAsync", "GetValueOrAsync" };

    private static readonly Dictionary<string, string> FamilyOf = new(StringComparer.Ordinal)
    {
        ["Bind"] = "Bind",
        ["BindWiden"] = "Bind",
        ["Map"] = "Map",
        ["Ensure"] = "Ensure",
        ["FailIf"] = "Ensure",
        ["MapError"] = "MapError",
        ["Compensate"] = "Compensate",
        ["Tap"] = "Tap",
        ["TapIf"] = "TapIf",
        ["TapError"] = "TapError",
        ["TryCatch"] = "TryCatch",
        ["Finally"] = "Finally",
        ["Select"] = "Select",
    };

    private static readonly Type[] TypedResults = [typeof(Result<,>), typeof(Result<,,>), typeof(Result<,,,>)];

    private static readonly Lazy<Dictionary<string, (bool Net9Only, int Priority)>> Expected = new(BuildExpected);

    private static readonly Lazy<Dictionary<string, MethodInfo>> Surface = new(BuildSurface);

    private static Dictionary<string, (bool, int)> BuildExpected()
    {
        var expected = new Dictionary<string, (bool, int)>(StringComparer.Ordinal);
        foreach (Type type in TypedResults)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (!FamilyOf.TryGetValue(method.Name, out string? family))
                    continue;
                if (family == "Finally" && method.ReturnType == type)
                    family = "FinallyAction";
                foreach (Cell cell in Cells[family])
                    expected.Add(ExpectedSignature(type, method, family, cell), (cell.Net9Only, ExpectedPriority(type, method, family, cell)));
            }
        }
        return expected;
    }

    private static int ExpectedPriority(Type type, MethodInfo method, string family, Cell cell)
    {
        int priority = cell.Prioritized ? 1 : 0;
        bool sameSlotExtension = family == "Ensure" && cell.Receiver != "this" && !method.IsGenericMethodDefinition && type.GetGenericArguments().Length < 4;
        bool slotTapError = family == "TapError" && cell.Receiver != "this";
        return sameSlotExtension || slotTapError ? priority + 2 : priority;
    }

    private static string ExpectedSignature(Type type, MethodInfo method, string family, Cell cell)
    {
        ParameterInfo[] parameters = method.GetParameters();
        int index = Array.FindLastIndex(parameters, p => p.ParameterType.Name.StartsWith("Func", StringComparison.Ordinal) || p.ParameterType.Name.StartsWith("Action", StringComparison.Ordinal));
        Type delegateType = parameters[index].ParameterType;
        Type[] handler = delegateType.GetGenericArguments();
        string handlerType;
        if (delegateType.Name.StartsWith("Action", StringComparison.Ordinal))
        {
            handlerType = cell.Handler == "sync" ? Format(delegateType) : $"Func<{Format(handler[0])}, {cell.Handler}>";
        }
        else
        {
            string ret = Format(handler[1]);
            handlerType = $"Func<{Format(handler[0])}, {(cell.Handler == "sync" ? ret : $"{cell.Handler}<{ret}>")}>";
        }
        IEnumerable<string> leading = parameters.Take(index).Select(p => Format(p.ParameterType));
        IEnumerable<string> extras = parameters.Skip(index + 1).Select(p => Format(p.ParameterType));
        string returns = Format(method.ReturnType);
        string[] methodGenerics = method.GetGenericArguments().Select(Format).ToArray();
        if (cell.Receiver == "this")
        {
            string generics = methodGenerics.Length == 0 ? string.Empty : $"<{string.Join(", ", methodGenerics)}>";
            IEnumerable<string> token = family == "Bind" ? [] : ["CancellationToken"];
            return $"{Format(type)}: {cell.Handler}<{returns}> {method.Name}Async{generics}({string.Join(", ", [.. leading, handlerType, .. extras, .. token])})";
        }
        string[] allGenerics = [.. type.GetGenericArguments().Select(Format), .. methodGenerics];
        string source = $"{cell.Receiver}<{Format(type)}>";
        return $"ResultExtensions: {cell.Receiver}<{returns}> {method.Name}Async<{string.Join(", ", allGenerics)}>({string.Join(", ", [source, .. leading, handlerType, .. extras, "CancellationToken"])})";
    }

    private static Dictionary<string, MethodInfo> BuildSurface()
    {
        var surface = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
        foreach (Type type in TypedResults)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.Name.EndsWith("Async", StringComparison.Ordinal))
                    surface.Add(Signature(type, method), method);
            }
        }
        foreach (MethodInfo method in typeof(ResultExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            Type source = method.GetParameters()[0].ParameterType;
            bool pending = source.IsGenericType && (source.GetGenericTypeDefinition() == typeof(Task<>) || source.GetGenericTypeDefinition() == typeof(ValueTask<>));
            if (pending && source.GetGenericArguments()[0] is { IsGenericType: true } inner
                && TypedResults.Contains(inner.GetGenericTypeDefinition())
                && !WithoutCellTable.Contains(method.Name) && !HasErrorSlotConstraint(method))
                surface.Add(Signature(typeof(ResultExtensions), method), method);
        }
        return surface;
    }

    private static bool HasErrorSlotConstraint(MethodInfo method) =>
        method.GetGenericArguments().Any(argument => argument.GetGenericParameterConstraints().Contains(typeof(CSharpEssentials.Errors.IError)));

    private static string Signature(Type declaringType, MethodInfo method)
    {
        string generics = method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(Format)) + ">"
            : string.Empty;
        string parameters = string.Join(", ", method.GetParameters().Select(p => Format(p.ParameterType)));
        return $"{Format(declaringType)}: {Format(method.ReturnType)} {method.Name}{generics}({parameters})";
    }

    private static string Format(Type type)
    {
        if (type == typeof(bool))
            return "bool";
        if (!type.IsGenericType)
            return type.Name;
        string name = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)];
        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(Format)) + ">";
    }

    private static int? Priority(MethodInfo method) =>
        method.GetCustomAttributesData()
            .Where(a => a.AttributeType.FullName == PriorityAttribute)
            .Select(a => (int?)a.ConstructorArguments[0].Value)
            .FirstOrDefault();

#if TYPED_NETSTANDARD_ASSET
    private const bool HasNet9Cells = false;
#else
    private const bool HasNet9Cells = true;
#endif

    [Fact]
    public void Matrix_Should_FindEverySyncFamily_When_ReadingTheSyncSurface()
    {
        IEnumerable<string> names = TypedResults
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Select(method => method.Name)
            .Where(FamilyOf.ContainsKey)
            .Distinct(StringComparer.Ordinal);

        names.Should().BeEquivalentTo(FamilyOf.Keys);
    }

    [Fact]
    public void Matrix_Should_ContainEveryCell_When_InspectingPublicSurface()
    {
        IEnumerable<string> missing = Expected.Value
            .Where(cell => HasNet9Cells || !cell.Value.Net9Only)
            .Select(cell => cell.Key)
            .Where(cell => !Surface.Value.ContainsKey(cell));

        missing.Should().BeEmpty();
    }

    [Fact]
    public void Matrix_Should_ContainNoOtherAsyncMember_When_InspectingPublicSurface()
    {
        IEnumerable<string> unexpected = Surface.Value.Keys
            .Where(member => !Expected.Value.TryGetValue(member, out (bool Net9Only, int) cell) || (cell.Net9Only && !HasNet9Cells));

        unexpected.Should().BeEmpty();
    }

    [Fact]
    public void Matrix_Should_GiveEachCellItsPriority_When_InspectingPublicSurface()
    {
        IEnumerable<string> wrong = Expected.Value
            .Where(cell => Surface.Value.ContainsKey(cell.Key))
            .Where(cell => (Priority(Surface.Value[cell.Key]) ?? 0) != cell.Value.Priority)
            .Select(cell => cell.Key);

        wrong.Should().BeEmpty();
    }

    [Fact]
    public void Recover_Should_StaySynchronous_When_InspectingPublicSurface()
    {
        IEnumerable<string> recover = Surface.Value.Keys.Where(member => member.Contains(" RecoverAsync", StringComparison.Ordinal));

        recover.Should().BeEmpty();
    }
}
