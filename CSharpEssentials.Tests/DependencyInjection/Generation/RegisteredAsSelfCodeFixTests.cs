using System.Collections.Immutable;
using CSharpEssentials.DependencyInjection.CodeFixes;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace CSharpEssentials.Tests.DependencyInjection.Generation;

public class RegisteredAsSelfCodeFixTests
{
    private const string DiagnosticId = "CSE2003";

    private const string Prelude = """
        using System;
        using CSharpEssentials.DependencyInjection;

        namespace Sample;

        public interface IFoo;

        public interface IClock;

        public interface IRepository<T>;

        """;

    public static TheoryData<string, string, string> Fixes => new()
    {
        { "[RegisterScoped] public sealed class Worker : IFoo, IClock;", "Register as 'IClock'", "[RegisterScoped(typeof(IClock))]" },
        { "[RegisterScoped] public sealed class Worker : IFoo, IClock;", "Register as 'IFoo'", "[RegisterScoped(typeof(IFoo))]" },
        { "[RegisterScoped] public sealed class Worker : IFoo, IClock;", "Register as self (As = ServiceAs.Self)", "[RegisterScoped(As = ServiceAs.Self)]" },
        { "[RegisterSingleton(Key = \"k\")] public sealed class Worker : IFoo;", "Register as 'IFoo'", "[RegisterSingleton(typeof(IFoo), Key = \"k\")]" },
        { "[RegisterSingleton(Key = \"k\")] public sealed class Worker : IFoo;", "Register as self (As = ServiceAs.Self)", "[RegisterSingleton(Key = \"k\", As = ServiceAs.Self)]" },
        { "[RegisterTransient] public sealed class SqlStore<T> : IRepository<T>;", "Register as 'IRepository<>'", "[RegisterTransient(typeof(IRepository<>))]" },
    };

    [Fact]
    public async Task CodeFix_Should_Offer_One_Action_Per_Interface_And_Self()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, "[RegisterScoped] public sealed class Worker : IFoo, IClock, IDisposable { public void Dispose() { } }");

        IReadOnlyList<CodeAction> actions = await GetActionsAsync(project);

        actions.Select(static action => action.Title).Should().Equal(
            "Register as 'IClock'",
            "Register as 'IFoo'",
            "Register as self (As = ServiceAs.Self)");
    }

    [Theory]
    [MemberData(nameof(Fixes))]
    public async Task CodeFix_Should_Update_Attribute_And_Clear_Diagnostic(string source, string title, string expectedAttribute)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, source);
        IReadOnlyList<CodeAction> actions = await GetActionsAsync(project);

        Project fixedProject = await CodeFixHarness.ApplyAsync(project, actions.Single(action => action.Title == title));

        string attribute = source[..(source.IndexOf(']', StringComparison.Ordinal) + 1)];
        (await CodeFixHarness.TextAsync(fixedProject, "Source0.cs")).Should().Be(Prelude + source.Replace(attribute, expectedAttribute, StringComparison.Ordinal));
        (await CodeFixHarness.GetDiagnosticsAsync(fixedProject, ServiceGeneration.Analyzers, DiagnosticId)).Should().BeEmpty();
        (await CodeFixHarness.GetCompilerErrorsAsync(fixedProject)).Should().BeEmpty();
    }

    private static Project CreateProject(AdhocWorkspace workspace, string source) =>
        CodeFixHarness.CreateProject(workspace, ServiceGeneration.CreateCompilation(Prelude + source));

    private static async Task<IReadOnlyList<CodeAction>> GetActionsAsync(Project project)
    {
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, ServiceGeneration.Analyzers, DiagnosticId);
        return await CodeFixHarness.GetActionsAsync(project, diagnostics.Single(), new RegisteredAsSelfCodeFixProvider());
    }
}
