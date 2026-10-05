using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

/// <summary>
/// Documents how the framework binders treat enum query/route values WITHOUT <c>UseEnumBinding()</c>.
/// These tests pin the reality the middleware exists to fix; if one fails after a framework upgrade,
/// the binder behavior changed.
/// </summary>
public class EnumBindingBaselineTests
{
    private static Task<EnumBindingHost> Start(EbHostKind kind) => EnumBindingHost.StartAsync(kind, useEnumBinding: false);

    // ---------- Minimal API (Enum.TryParse, case-sensitive, no IsDefined check) ----------

    [Fact]
    public async Task Baseline_MinimalApi_MemberName_IsAccepted()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?status=InProgress");

        response.Body.Should().Be("InProgress");
    }

    [Theory]
    [InlineData("inprogress")]
    [InlineData("INPROGRESS")]
    public async Task Baseline_MinimalApi_MemberNameWithDifferentCase_IsRejected(string value)
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync($"/status?status={value}");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_MinimalApi_SnakeCase_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?status=in_progress");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_MinimalApi_SnakeCaseRouteValue_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/items/in_progress");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_MinimalApi_Rejection_HasEmptyBodyNotProblemJson()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?status=garbage");

        response.Body.Should().BeEmpty();
    }

    [Fact]
    public async Task Baseline_MinimalApi_DefinedNumber_IsAccepted()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?status=1");

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task Baseline_MinimalApi_UndefinedNumber_IsAcceptedAsRawValue()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?status=99");

        response.Body.Should().Be("99");
    }

    [Fact]
    public async Task Baseline_MinimalApi_NonFlagsCommaList_IsOredIntoAMember()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?status=Active,InProgress");

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task Baseline_MinimalApi_FlagsLowercaseList_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/flags?perms=read,write");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_MinimalApi_FlagsUndefinedBits_AreAccepted()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/flags?perms=99");

        response.Body.Should().Be("99");
    }

    [Fact]
    public async Task Baseline_MinimalApi_JsonStringEnumMemberName_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/custom?value=custom");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_MinimalApi_NullableEmptyValue_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/nullable?status=");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_MinimalApi_QueryKey_IsCaseInsensitive()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MinimalApi);

        var response = await host.GetAsync("/status?STATUS=InProgress");

        response.Body.Should().Be("InProgress");
    }

    // ---------- MVC (EnumTypeModelBinder: case-insensitive, IsDefined check) ----------

    [Theory]
    [InlineData("inprogress")]
    [InlineData("INPROGRESS")]
    public async Task Baseline_Mvc_MemberNameWithDifferentCase_IsAccepted(string value)
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync($"/status?status={value}");

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task Baseline_MvcApiController_SnakeCase_IsRejectedWithModelStateProblem()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/status?status=in_progress");

        response.Status.Should().Be(400);
        response.Body.Should().Contain("The value 'in_progress' is not valid.");
    }

    [Fact]
    public async Task Baseline_MvcController_SnakeCase_RunsActionWithInvalidModelState()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcController);

        var response = await host.GetAsync("/status?status=in_progress");

        response.Body.Should().Be("invalid");
    }

    [Fact]
    public async Task Baseline_Mvc_DefinedNumber_IsAccepted()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/status?status=1");

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task Baseline_Mvc_UndefinedNumber_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/status?status=99");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_Mvc_NonFlagsCommaList_IsOredIntoAMember()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/status?status=Active,InProgress");

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task Baseline_Mvc_FlagsLowercaseList_IsAccepted()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/flags?perms=read,write");

        response.Body.Should().Be("Read, Write");
    }

    [Fact]
    public async Task Baseline_Mvc_FlagsUndefinedBits_AreRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/flags?perms=99");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_Mvc_JsonStringEnumMemberName_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/custom?value=custom");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_Mvc_NullableEmptyValue_BindsNull()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/nullable?status=");

        response.Body.Should().Be("null");
    }

    [Fact]
    public async Task Baseline_Mvc_NonNullableEmptyValue_IsRejected()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/status?status=");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task Baseline_Mvc_MissingNonNullableValue_BindsDefaultMember()
    {
        await using EnumBindingHost host = await Start(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/status");

        response.Body.Should().Be("Active");
    }
}
