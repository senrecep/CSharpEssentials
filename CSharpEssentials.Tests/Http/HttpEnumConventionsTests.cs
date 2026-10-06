using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Http;
using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpEnumConventionsTests
{
    private static readonly EnumConventions Conventions = EnumConventions.Default;

    [Fact]
    public void ToQueryString_Should_Write_Wire_Names()
    {
        Result<string> result = new { Status = HttpOrderStatus.PendingApproval, Page = 2 }.ToQueryString();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Status=pending_approval&Page=2");
    }

    [Fact]
    public void ToQueryString_Should_Write_Numbers_With_Number_Format()
    {
        Result<string> result = new { Status = HttpOrderStatus.PendingApproval }.ToQueryString(Conventions, EnumWireFormat.Number);

        result.Value.Should().Be("Status=1");
    }

    [Fact]
    public void ToQueryString_Should_Use_WriteAs_Of_The_Conventions()
    {
        Result<string> result = new { Status = HttpOrderStatus.Pending }.ToQueryString(Conventions with { WriteAs = EnumWireFormat.Number });

        result.Value.Should().Be("Status=0");
    }

    [Fact]
    public void ToQueryString_Should_Repeat_The_Key_For_Flags_And_Collections()
    {
        var source = new
        {
            Permissions = HttpPermissions.ReadWrite | HttpPermissions.Delete,
            Status = new List<HttpOrderStatus?> { HttpOrderStatus.Pending, null, HttpOrderStatus.PendingApproval },
        };

        Result<string> result = source.ToQueryString();

        result.Value.Should().Be("Permissions=read&Permissions=write&Permissions=delete&Status=pending&Status=pending_approval");
    }

    [Fact]
    public void ToQueryString_Should_Write_The_Bitmask_For_Flags_With_Number_Format()
    {
        Result<string> result = new { Permissions = HttpPermissions.ReadWrite }.ToQueryString(Conventions, EnumWireFormat.Number);

        result.Value.Should().Be("Permissions=3");
    }

    [Fact]
    public void ToQueryString_Should_Skip_Empty_Flags_And_Null_Values()
    {
        Result<string> result = new { Permissions = HttpPermissions.None, Status = (HttpOrderStatus?)null }.ToQueryString();

        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void ToQueryString_Should_Fail_Instead_Of_Sending_An_Undefined_Value()
    {
        Result<string> result = new { Status = (HttpOrderStatus)42 }.ToQueryString();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("QueryString.InvalidEnumValue");
    }

    [Fact]
    public void ToQueryString_Should_Fall_Back_To_ToString_For_Plain_Enums_Without_Throwing()
    {
        Result<string> names = new { Status = HttpPlainStatus.Approved }.ToQueryString();
        Result<string> numbers = new { Status = HttpPlainStatus.Approved }.ToQueryString(Conventions, EnumWireFormat.Number);

        names.Value.Should().Be("Status=Approved");
        numbers.Value.Should().Be("Status=Approved");
    }

    [Fact]
    public void WithQueryString_Should_Format_A_Single_Enum_Value()
    {
        Result<Uri> result = new Uri("http://localhost/orders?page=1").WithQueryString("status", HttpOrderStatus.PendingApproval);

        result.Value.Query.Should().Be("?page=1&status=pending_approval");
    }

    [Fact]
    public void WithQueryString_Should_Format_Flags_As_Repeated_Keys_With_Explicit_Number_Format()
    {
        Uri uri = new("http://localhost/orders");

        uri.WithQueryString("permissions", HttpPermissions.ReadWrite).Value.Query.Should().Be("?permissions=read&permissions=write");
        uri.WithQueryString("permissions", HttpPermissions.ReadWrite, Conventions, EnumWireFormat.Number).Value.Query.Should().Be("?permissions=3");
    }

    [Fact]
    public void WithQueryString_Should_Format_Enum_Properties_Of_An_Object_With_Conventions()
    {
        Result<Uri> result = new Uri("http://localhost/orders").WithQueryString(new { Status = HttpOrderStatus.PendingApproval }, Conventions, EnumWireFormat.Number);

        result.Value.Query.Should().Be("?Status=1");
    }

    [Fact]
    public void WithQueryString_Should_Keep_Strings_Unchanged()
    {
        Result<Uri> result = new Uri("http://localhost/orders").WithQueryString("name", (object?)"a b");

        result.Value.Query.Should().Be("?name=a%20b");
    }

    [Fact]
    public void Builder_Should_Format_Enum_Query_Values_And_Repeat_Keys()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders")
            .WithQuery("status", HttpOrderStatus.PendingApproval)
            .WithQuery("permissions", HttpPermissions.Read | HttpPermissions.Delete)
            .WithQuery("history", new[] { HttpOrderStatus.Pending, HttpOrderStatus.PendingApproval })
            .WithQuery("page", "1")
            .Build();

        using HttpRequestMessage request = result.Value;
        request.RequestUri!.Query.Should().Be("?status=pending_approval&permissions=read&permissions=delete&history=pending&history=pending_approval&page=1");
    }

    [Fact]
    public void Builder_Should_Write_Numbers_When_The_Client_Is_Configured_With_Number()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders/{status}")
            .WithEnumConventions(Conventions, EnumWireFormat.Number)
            .WithRoute("status", HttpOrderStatus.PendingApproval)
            .WithQuery("permissions", HttpPermissions.ReadWrite)
            .Build();

        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsolutePath.Should().Be("/orders/1");
        request.RequestUri.Query.Should().Be("?permissions=3");
    }

    [Fact]
    public void Builder_Should_Use_WriteAs_Of_The_Conventions_When_No_Format_Is_Given()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders/{status}")
            .WithEnumConventions(Conventions with { WriteAs = EnumWireFormat.Number })
            .WithRoute("status", HttpOrderStatus.PendingApproval)
            .Build();

        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsolutePath.Should().Be("/orders/1");
    }

    [Fact]
    public void Builder_Should_Format_Enum_Route_Values_With_Wire_Names()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders/{status}/items/{id}")
            .WithRoute("status", HttpOrderStatus.PendingApproval)
            .WithRoute("id", 7)
            .Build();

        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsolutePath.Should().Be("/orders/pending_approval/items/7");
    }

    [Fact]
    public void Builder_Should_Escape_Route_Values()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/search/{term}")
            .WithRoute("term", "a/b c")
            .Build();

        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsolutePath.Should().Be("/search/a%2Fb%20c");
    }

    [Fact]
    public void Builder_Should_Fail_When_The_Route_Placeholder_Is_Missing()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders")
            .WithRoute("status", HttpOrderStatus.Pending)
            .Build();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("HttpRequestBuilder.RouteParameterNotFound");
    }

    [Fact]
    public void Builder_Should_Fail_When_A_Route_Value_Is_Null()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders/{status}")
            .WithRoute("status", null)
            .Build();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("HttpRequestBuilder.RouteValueRequired");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Builder_Should_Fail_Instead_Of_Sending_An_Undefined_Enum_Value(bool inRoute)
    {
        HttpRequestBuilder builder = HttpRequestBuilder.Get("http://localhost/orders/{status}");
        builder = inRoute
            ? builder.WithRoute("status", (HttpOrderStatus)42)
            : builder.WithRoute("status", "x").WithQuery("status", (HttpOrderStatus)42);

        Result<HttpRequestMessage> result = builder.Build();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("HttpRequestBuilder.InvalidEnumValue");
    }

    [Fact]
    public void Builder_Should_Fall_Back_To_ToString_For_Plain_Enums_In_Route_And_Query()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("http://localhost/orders/{status}")
            .WithEnumConventions(Conventions, EnumWireFormat.Number)
            .WithRoute("status", HttpPlainStatus.Approved)
            .WithQuery("also", HttpPlainStatus.Pending)
            .Build();

        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsolutePath.Should().Be("/orders/Approved");
        request.RequestUri.Query.Should().Be("?also=Pending");
    }

    [Fact]
    public void TryFormat_Should_Return_False_For_Plain_Enums_Without_Throwing()
    {
        bool formatted = EnumValueFormatter.TryFormat(HttpPlainStatus.Approved, Conventions, out string? text);
        List<string> values = [];
        bool formattedMany = EnumValueFormatter.TryFormatMany(HttpPlainStatus.Approved, Conventions, values);

        formatted.Should().BeFalse();
        text.Should().BeNull();
        formattedMany.Should().BeFalse();
        values.Should().BeEmpty();
    }

    [Fact]
    public async Task Builder_Should_Write_Wire_Names_In_Json_Bodies_By_Default()
    {
        string json = await BodyAsync(HttpRequestBuilder.Post("http://localhost/orders")
            .WithJsonContent(new HttpOrderPayload(HttpOrderStatus.PendingApproval, HttpPermissions.ReadWrite, [HttpOrderStatus.Pending])));

        json.Should().Be("""{"status":"pending_approval","permissions":["read","write"],"history":["pending"]}""");
    }

    [Fact]
    public async Task Builder_Should_Apply_Enum_Conventions_Set_After_WithJsonContent()
    {
        string json = await BodyAsync(HttpRequestBuilder.Post("http://localhost/orders")
            .WithJsonContent(new HttpOrderPayload(HttpOrderStatus.PendingApproval, HttpPermissions.ReadWrite, [HttpOrderStatus.Pending]))
            .WithEnumConventions(Conventions, EnumWireFormat.Number));

        json.Should().Be("""{"status":1,"permissions":3,"history":[0]}""");
    }

    [Fact]
    public async Task Builder_Should_Write_Numbers_In_Json_Bodies_When_The_Client_Is_Configured_With_Number()
    {
        string json = await BodyAsync(HttpRequestBuilder.Post("http://localhost/orders")
            .WithEnumConventions(Conventions, EnumWireFormat.Number)
            .WithJsonContent(new HttpOrderPayload(HttpOrderStatus.PendingApproval, HttpPermissions.ReadWrite, [HttpOrderStatus.Pending])));

        json.Should().Be("""{"status":1,"permissions":3,"history":[0]}""");
    }

    [Fact]
    public async Task Builder_Should_Leave_Plain_Enums_To_The_Default_Number_Behavior_In_Json_Bodies()
    {
        string json = await BodyAsync(HttpRequestBuilder.Post("http://localhost/orders")
            .WithEnumConventions(Conventions)
            .WithJsonContent(new HttpPlainPayload(HttpPlainStatus.Approved)));

        json.Should().Be("""{"status":1}""");
    }

    [Fact]
    public async Task Builder_Should_Write_Plain_Enums_With_Reflection_Only_On_Explicit_Opt_In()
    {
        EnumConventions all = Conventions with { CanHandle = static type => type.IsEnum };
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventionsWithReflection(all);

        string json = await BodyAsync(HttpRequestBuilder.Post("http://localhost/orders")
            .WithJsonContent(new HttpPlainPayload(HttpPlainStatus.Approved), options));

        json.Should().Be("""{"status":"approved"}""");
    }

    private static async Task<string> BodyAsync(HttpRequestBuilder builder)
    {
        Result<HttpRequestMessage> result = builder.Build();
        using HttpRequestMessage request = result.Value;
        return await request.Content!.ReadAsStringAsync();
    }
}
