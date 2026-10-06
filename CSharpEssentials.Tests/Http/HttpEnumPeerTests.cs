using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Http;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpEnumPeerTests
{
    private static readonly HttpOrderPayload Payload = new(
        HttpOrderStatus.PendingApproval,
        HttpPermissions.ReadWrite,
        [HttpOrderStatus.Pending, HttpOrderStatus.PendingApproval]);

    [Fact]
    public async Task String_Client_Should_Send_Wire_Names_To_A_Number_Server_And_Read_Its_Numbers()
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.Number);
        PeerClient client = new(server.Client, EnumConventions.Default, EnumWireFormat.String);

        Result<HttpEchoResponse> route = await client.RouteAsync(HttpOrderStatus.PendingApproval);
        Result<HttpEchoResponse> query = await client.QueryAsync(HttpOrderStatus.PendingApproval, HttpPermissions.ReadWrite);
        Result<HttpBodyEchoResponse> body = await client.BodyAsync(Payload);

        route.Value.Raw.Should().Equal("pending_approval");
        route.Value.Parsed.Should().Equal(1);
        query.Value.Raw.Should().Equal("pending_approval", "read", "write");
        query.Value.Parsed.Should().Equal(1, 3);
        body.Value.Raw.GetRawText().Should().Be("""{"status":"pending_approval","permissions":["read","write"],"history":["pending","pending_approval"]}""");
        body.Value.Order.Should().BeEquivalentTo(Payload);
    }

    [Fact]
    public async Task Number_Client_Should_Send_Numbers_To_A_Tolerant_Server_And_Read_Its_Wire_Names()
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, EnumWireFormat.Number);

        Result<HttpEchoResponse> route = await client.RouteAsync(HttpOrderStatus.PendingApproval);
        Result<HttpEchoResponse> query = await client.QueryAsync(HttpOrderStatus.PendingApproval, HttpPermissions.ReadWrite);
        Result<HttpBodyEchoResponse> body = await client.BodyAsync(Payload);

        route.Value.Raw.Should().Equal("1");
        route.Value.Parsed.Should().Equal(1);
        query.Value.Raw.Should().Equal("1", "3");
        query.Value.Parsed.Should().Equal(1, 3);
        body.Value.Raw.GetRawText().Should().Be("""{"status":1,"permissions":3,"history":[0,1]}""");
        body.Value.Order.Should().BeEquivalentTo(Payload);
    }

    [Theory]
    [InlineData(EnumWireFormat.String, EnumWireFormat.String)]
    [InlineData(EnumWireFormat.String, EnumWireFormat.Number)]
    [InlineData(EnumWireFormat.Number, EnumWireFormat.String)]
    [InlineData(EnumWireFormat.Number, EnumWireFormat.Number)]
    public async Task Every_Client_And_Server_Output_Combination_Should_Round_Trip_Every_Defined_Value(EnumWireFormat clientWriteAs, EnumWireFormat serverWriteAs)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(serverWriteAs);
        PeerClient client = new(server.Client, EnumConventions.Default, clientWriteAs);

        foreach (HttpOrderStatus status in new[] { HttpOrderStatus.Pending, HttpOrderStatus.PendingApproval })
        {
            Result<HttpEchoResponse> route = await client.RouteAsync(status);
            Result<HttpEchoResponse> query = await client.QueryAsync(status, null);
            Result<HttpBodyEchoResponse> body = await client.BodyAsync(Payload with { Status = status });

            route.Value.Parsed.Should().Equal((long)status);
            query.Value.Parsed.Should().Equal((long)status);
            body.Value.Order.Status.Should().Be(status);
        }
    }

    [Theory]
    [InlineData(EnumWireFormat.String)]
    [InlineData(EnumWireFormat.Number)]
    public async Task The_Fallback_Member_Should_Be_Rejected_By_The_Server_In_Route_Query_And_Body(EnumWireFormat clientWriteAs)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, clientWriteAs);

        Result<HttpEchoResponse> route = await client.RouteAsync(HttpOrderStatus.Unknown);
        Result<HttpEchoResponse> query = await client.QueryAsync(HttpOrderStatus.Unknown, null);
        Result<HttpBodyEchoResponse> body = await client.BodyAsync(Payload with { Status = HttpOrderStatus.Unknown });

        route.IsFailure.Should().BeTrue();
        query.IsFailure.Should().BeTrue();
        body.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumWireFormat.String)]
    [InlineData(EnumWireFormat.Number)]
    public async Task An_Undefined_Value_Should_Fail_The_Call_Before_It_Is_Sent(EnumWireFormat clientWriteAs)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, clientWriteAs);
        HttpOrderStatus undefined = (HttpOrderStatus)42;

        Result<HttpEchoResponse> route = await client.RouteAsync(undefined);
        Result<HttpEchoResponse> query = await client.QueryAsync(undefined, null);
        Result<HttpBodyEchoResponse> body = await client.BodyAsync(Payload with { Status = undefined });

        route.IsFailure.Should().BeTrue();
        route.Errors[0].Code.Should().Be("HttpRequestBuilder.InvalidEnumValue");
        query.IsFailure.Should().BeTrue();
        query.Errors[0].Code.Should().Be("HttpRequestBuilder.InvalidEnumValue");
        body.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumWireFormat.String, new[] { "read", "write", "delete" })]
    [InlineData(EnumWireFormat.Number, new[] { "7" })]
    public async Task Flags_Should_Be_Sent_As_Repeated_Query_Keys_Or_One_Number(EnumWireFormat clientWriteAs, string[] expectedRaw)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, clientWriteAs);

        Result<HttpEchoResponse> query = await client.QueryAsync(null, HttpPermissions.ReadWrite | HttpPermissions.Delete);

        query.Value.Raw.Should().Equal(expectedRaw);
        query.Value.Parsed.Should().Equal(7);
    }

    [Fact]
    public async Task Enum_Collections_Should_Be_Sent_As_Repeated_Query_Keys()
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, EnumWireFormat.String);

        Result<HttpEchoResponse> query = await client.QueryAsync(new List<HttpOrderStatus> { HttpOrderStatus.Pending, HttpOrderStatus.PendingApproval }, null);

        query.Value.Raw.Should().Equal("pending", "pending_approval");
        query.Value.Parsed.Should().Equal(0, 1);
    }

    public static TheoryData<string, HttpOrderStatus> TolerantDataReads => new()
    {
        { "\"pending_approval\"", HttpOrderStatus.PendingApproval },
        { "\"PendingApproval\"", HttpOrderStatus.PendingApproval },
        { "\"PENDING_APPROVAL\"", HttpOrderStatus.PendingApproval },
        { "\"pendingApproval\"", HttpOrderStatus.PendingApproval },
        { "\"Approval\"", HttpOrderStatus.PendingApproval },
        { "\"approval\"", HttpOrderStatus.PendingApproval },
        { "1", HttpOrderStatus.PendingApproval },
        { "\"1\"", HttpOrderStatus.PendingApproval },
        { "\"unknown\"", HttpOrderStatus.Unknown },
        { "99999", HttpOrderStatus.Unknown },
        { "99", HttpOrderStatus.Unknown },
        { "\"99\"", HttpOrderStatus.Unknown },
        { "\"bogus\"", HttpOrderStatus.Unknown },
    };

    [Theory]
    [MemberData(nameof(TolerantDataReads))]
    public async Task Response_Bodies_Should_Be_Read_Tolerantly_With_Fallback(string json, HttpOrderStatus expected)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, EnumWireFormat.String);

        Result<HttpStatusResponse> result = await client.StatusAsync(json);

        result.Value.Status.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TolerantDataReads))]
    public async Task Response_Bodies_Should_Be_Read_Tolerantly_Even_When_Input_Is_Restricted(string json, HttpOrderStatus expected)
    {
        EnumConventions restricted = EnumConventions.Default with { AcceptNumbers = false, AcceptMemberNames = false, CaseInsensitive = false };
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, restricted, EnumWireFormat.String);

        Result<HttpStatusResponse> result = await client.StatusAsync(json);

        result.Value.Status.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\" pending\"")]
    [InlineData("\"1.0\"")]
    [InlineData("\"0x1\"")]
    [InlineData("\"+1\"")]
    [InlineData("\"-0\"")]
    public async Task Malformed_Response_Values_Should_Fail_The_Read(string json)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, EnumConventions.Default, EnumWireFormat.String);

        Result<HttpStatusResponse> result = await client.StatusAsync(json);

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData("99")]
    [InlineData("\"99\"")]
    [InlineData("\"bogus\"")]
    public async Task Undefined_Response_Values_Should_Fail_The_Read_When_Unknown_Values_Are_Rejected(string json)
    {
        EnumConventions rejecting = EnumConventions.Default with { UnknownValue = UnknownEnumValueHandling.Reject };
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(EnumWireFormat.String);
        PeerClient client = new(server.Client, rejecting, EnumWireFormat.String);

        Result<HttpStatusResponse> result = await client.StatusAsync(json);

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumWireFormat.String)]
    [InlineData(EnumWireFormat.Number)]
    public async Task Clients_Should_Read_Both_Server_Output_Formats(EnumWireFormat serverWriteAs)
    {
        await using HttpEnumPeerHost server = await HttpEnumPeerHost.StartAsync(serverWriteAs);
        PeerClient client = new(server.Client, EnumConventions.Default, EnumWireFormat.String);

        Result<HttpStatusResponse> result = await HttpRequestBuilder.Get("http://localhost/status/current")
            .AsResultAsync<HttpStatusResponse>(server.Client, client.Json);

        result.Value.Status.Should().Be(HttpOrderStatus.PendingApproval);
    }

    private sealed class PeerClient(HttpClient http, EnumConventions conventions, EnumWireFormat writeAs)
    {
        public JsonSerializerOptions Json { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .AddEnumConventions(conventions, EnumReadMode.Data, writeAs);

        public Task<Result<HttpEchoResponse>> RouteAsync(object status) =>
            HttpRequestBuilder.Get("http://localhost/route/{status}")
                .WithEnumConventions(conventions, writeAs)
                .WithRoute("status", status)
                .AsResultAsync<HttpEchoResponse>(http, Json);

        public Task<Result<HttpEchoResponse>> QueryAsync(object? status, object? permissions) =>
            HttpRequestBuilder.Get("http://localhost/query")
                .WithEnumConventions(conventions, writeAs)
                .WithQuery("status", status)
                .WithQuery("permissions", permissions)
                .AsResultAsync<HttpEchoResponse>(http, Json);

        public Task<Result<HttpBodyEchoResponse>> BodyAsync(HttpOrderPayload payload) =>
            HttpRequestBuilder.Post("http://localhost/body")
                .WithEnumConventions(conventions, writeAs)
                .WithJsonContent(payload, Json)
                .AsResultAsync<HttpBodyEchoResponse>(http, Json);

        public Task<Result<HttpStatusResponse>> StatusAsync(string json) =>
            HttpRequestBuilder.Get("http://localhost/status")
                .WithQuery("value", json)
                .AsResultAsync<HttpStatusResponse>(http, Json);
    }
}
