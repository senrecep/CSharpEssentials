using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

/// <summary>
/// The binding matrix of <c>UseEnumBinding()</c>: route, query, header and form values, scalar, nullable, array and flags,
/// for Minimal API (<c>/min</c>) and an MVC controller (<c>/mvc</c>) in the same host.
/// </summary>
public class EnumConventionsBindingMatrixTests
{
    private const string AllowedStatuses = "Allowed values: pending, pending_approval, shipped.";

    public static TheoryData<string, string> ScalarRows => Rows(["route", "query", "header", "form"]);

    public static TheoryData<string, string> CollectionRows => Rows(["query", "header", "form"]);

    public static TheoryData<string, string> FlagsRows => Rows(["route", "query", "header", "form"]);

    public static TheoryData<string, string, string> AcceptedScalars
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (string api in new[] { "min", "mvc" })
                foreach (string source in new[] { "route", "query", "header", "form" })
                    foreach (string value in new[] { "pending_approval", "PENDING_APPROVAL", "PendingApproval", "pendingapproval", "1", " pending_approval", "PendingApproval ", " 1 " })
                        data.Add(api, source, value);
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(AcceptedScalars))]
    public async Task Scalar_Should_Bind_When_ValueIsAccepted(string api, string source, string value)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "", value);

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("PendingApproval");
    }

    [Theory]
    [MemberData(nameof(ScalarRows))]
    public async Task Scalar_Should_Return400ListingAllowedValues_When_NumberIsUndefined(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "", "99");

        response.ShouldBeProblem().Should().ContainSingle().Which.Should().Be(
            (StatusKey(source), $"'99' is not a valid EcStatus. {AllowedStatuses}"));
    }

    [Theory]
    [MemberData(nameof(ScalarRows))]
    public async Task Scalar_Should_Return400_When_ValueIsUnknown(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "", "garbage");

        response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().EndWith(AllowedStatuses);
    }

    [Theory]
    [MemberData(nameof(Apis))]
    public async Task Query_Should_Return400ListingAllowedValues_When_StatusIs99(string api)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.GetAsync($"/{api}/query?status=99");

        response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().Contain("pending_approval");
    }

    [Theory]
    [MemberData(nameof(NullableRows))]
    public async Task Nullable_Should_BindNull_When_ValueIsMissing(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "-nullable");

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("null");
    }

    [Theory]
    [MemberData(nameof(NullableRows))]
    public async Task Nullable_Should_BindValue_When_ValueIsPresent(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "-nullable", "shipped");

        response.Body.Should().Be("Shipped");
    }

    [Theory]
    [MemberData(nameof(Apis))]
    public async Task NullableQuery_Should_BindNull_When_ValueIsEmpty(string api)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.GetAsync($"/{api}/query-nullable?status=");

        response.Body.Should().Be("null");
    }

    [Theory]
    [MemberData(nameof(Apis))]
    public async Task NullableForm_Should_BindNull_When_ValueIsEmpty(string api)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.PostFormAsync($"/{api}/form-nullable", ("status", ""));

        response.Body.Should().Be("null");
    }

    [Theory]
    [MemberData(nameof(NullableRows))]
    public async Task Nullable_Should_Return400_When_ValueIsInvalid(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "-nullable", "99");

        response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().EndWith(AllowedStatuses);
    }

    [Theory]
    [MemberData(nameof(CollectionRows))]
    public async Task Array_Should_BindEveryValue_When_KeyIsRepeated(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "-array", "pending", "PENDING_APPROVAL", "2");

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("Pending|PendingApproval|Shipped");
    }

    [Theory]
    [MemberData(nameof(CollectionRows))]
    public async Task Array_Should_BindEveryValue_When_ValueIsCommaSeparated(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "-array", "pending,PENDING_APPROVAL, shipped");

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("Pending|PendingApproval|Shipped");
    }

    [Theory]
    [MemberData(nameof(CollectionRows))]
    public async Task Array_Should_Return400_When_AnItemIsInvalid(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "-array", "pending", "99");

        response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().Be(
            $"'99' is not a valid EcStatus. {AllowedStatuses}");
    }

    [Theory]
    [MemberData(nameof(FlagsRows))]
    public async Task Flags_Should_CombineMembers_When_ValueIsCommaSeparated(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await SendFlags(host, api, source, "read,WRITE");

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("Read, Write");
    }

    [Theory]
    [MemberData(nameof(FlagsRows))]
    public async Task Flags_Should_BindNumber_When_NumberIsACombination(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await SendFlags(host, api, source, "5");

        response.Body.Should().Be("Read, Delete");
    }

    [Theory]
    [MemberData(nameof(FlagsRows))]
    public async Task Flags_Should_Return400_When_APartIsInvalid(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await SendFlags(host, api, source, "read,garbage");

        response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().Contain("is not a valid EcPermission");
    }

    [Theory]
    [MemberData(nameof(RequiredRows))]
    public async Task Required_Should_KeepFrameworkHandling_When_ValueIsMissing(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "");

        response.Body.Should().NotContain("is not a valid");
    }

    [Fact]
    public async Task MinimalApi_Should_Return400_When_RequiredQueryValueIsMissing()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.GetAsync("/min/query");

        response.Status.Should().Be(400);
    }

    [Theory]
    [MemberData(nameof(ScalarRows))]
    public async Task Scalar_Should_Return400_When_ValueIsInvalidAfterTrimming(string api, string source)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await Send(host, api, source, "", " bogus ");

        response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().EndWith(AllowedStatuses);
    }

    [Fact]
    public async Task Mvc_Should_Validate_Only_The_First_Source_Of_An_Unannotated_Parameter()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.GetAsync("/mvc/route/Shipped?status=bogus");

        response.Status.Should().Be(200);
    }

    public static TheoryData<string> Apis => new() { "min", "mvc" };

    public static TheoryData<string, string> NullableRows => Rows(["query", "header", "form"]);

    public static TheoryData<string, string> RequiredRows => Rows(["query", "header", "form"]);

    private static TheoryData<string, string> Rows(string[] sources)
    {
        var data = new TheoryData<string, string>();
        foreach (string api in new[] { "min", "mvc" })
            foreach (string source in sources)
                data.Add(api, source);
        return data;
    }

    private static string StatusKey(string source) => source == "header" ? "X-Status" : "status";

    /// <summary>Sends <paramref name="values"/> as the <c>status</c> value of the <paramref name="source"/> endpoint.</summary>
    private static Task<EcResponse> Send(EnumConventionsHost host, string api, string source, string suffix, params string[] values)
    {
        string path = $"/{api}/{source}{suffix}";
        return source switch
        {
            "route" => host.GetAsync($"{path}/{Uri.EscapeDataString(values.Single())}"),
            "query" => host.GetAsync(path + Query("status", values)),
            "header" => host.GetAsync(path, [.. values.Select(v => ("X-Status", v))]),
            // Minimal API rejects a form request without a body, so a missing value is sent next to an unrelated field.
            _ => host.PostFormAsync(path, [("other", "x"), .. values.Select(v => ("status", v))]),
        };
    }

    private static Task<EcResponse> SendFlags(EnumConventionsHost host, string api, string source, string value)
    {
        string path = $"/{api}/{source}-flags";
        return source switch
        {
            "route" => host.GetAsync($"{path}/{Uri.EscapeDataString(value)}"),
            "query" => host.GetAsync(path + Query("perms", [value])),
            "header" => host.GetAsync(path, ("X-Perms", value)),
            _ => host.PostFormAsync(path, ("perms", value)),
        };
    }

    private static string Query(string key, string[] values) =>
        values.Length == 0 ? "" : "?" + string.Join("&", values.Select(v => $"{key}={Uri.EscapeDataString(v)}"));
}
