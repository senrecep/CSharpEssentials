using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using CSharpEssentials.Tests.Enums;
using CSharpEssentials.Tests.Fixtures.EnumsContracts;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

/// <summary>
/// The JSON rows of the normative matrix (design section 5) in both read modes.
/// </summary>
public class EnumConverterFactoryTests
{
    private static JsonSerializerOptions Options(
        EnumReadMode mode = EnumReadMode.Data,
        EnumConventions? conventions = null,
        EnumWireFormat? writeAs = null) =>
        new JsonSerializerOptions().AddEnumConventions(conventions ?? EnumConventions.Default, mode, writeAs);

    private static T? Read<T>(string json, EnumReadMode mode, EnumConventions? conventions = null) =>
        JsonSerializer.Deserialize<T>(json, Options(mode, conventions));

    public static TheoryData<string, EnumReadMode> AcceptedSpellings()
    {
        TheoryData<string, EnumReadMode> data = [];
        foreach (EnumReadMode mode in new[] { EnumReadMode.Input, EnumReadMode.Data })
        {
            foreach (string json in new[]
            {
                "\"pending_approval\"", "\"PendingApproval\"", "\"PENDING_APPROVAL\"", "\"pendingApproval\"",
                "\"Approval\"", "\"approval\"", "1", "\"1\"",
            })
            {
                data.Add(json, mode);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AcceptedSpellings))]
    public void Read_Should_Accept_Every_Spelling_Of_A_Defined_Member(string json, EnumReadMode mode)
    {
        JsonOrderStatus value = Read<JsonOrderStatus>(json, mode);

        value.Should().Be(JsonOrderStatus.PendingApproval);
    }

    public static TheoryData<string, EnumConventions> InputSwitches => new()
    {
        { "1", EnumConventions.Default with { AcceptNumbers = false } },
        { "\"1\"", EnumConventions.Default with { AcceptNumbers = false } },
        { "\"PendingApproval\"", EnumConventions.Default with { AcceptMemberNames = false } },
        { "\"PENDING_APPROVAL\"", EnumConventions.Default with { CaseInsensitive = false } },
    };

    [Theory]
    [MemberData(nameof(InputSwitches))]
    public void Read_Should_Reject_A_Switched_Off_Spelling_In_Input_Mode(string json, EnumConventions conventions)
    {
        Action act = () => Read<JsonOrderStatus>(json, EnumReadMode.Input, conventions);

        act.Should().Throw<EnumValueJsonException>();
    }

    [Theory]
    [MemberData(nameof(InputSwitches))]
    public void Read_Should_Ignore_The_Input_Switches_In_Data_Mode(string json, EnumConventions conventions)
    {
        JsonOrderStatus value = Read<JsonOrderStatus>(json, EnumReadMode.Data, conventions);

        value.Should().Be(JsonOrderStatus.PendingApproval);
    }

    [Theory]
    [InlineData("99")]
    [InlineData("\"99\"")]
    [InlineData("\"bogus\"")]
    [InlineData("\"unknown\"")]
    [InlineData("99999")]
    public void Read_Should_Reject_An_Undefined_Value_Or_The_Fallback_Member_In_Input_Mode(string json)
    {
        Action act = () => Read<JsonOrderStatus>(json, EnumReadMode.Input);

        act.Should().Throw<EnumValueJsonException>();
    }

    [Theory]
    [InlineData("99")]
    [InlineData("\"99\"")]
    [InlineData("\"bogus\"")]
    [InlineData("\"unknown\"")]
    [InlineData("99999")]
    public void Read_Should_Read_An_Undefined_Value_As_The_Fallback_Member_In_Data_Mode(string json)
    {
        JsonOrderStatus value = Read<JsonOrderStatus>(json, EnumReadMode.Data);

        value.Should().Be(JsonOrderStatus.Unknown);
    }

    [Theory]
    [InlineData("99")]
    [InlineData("\"bogus\"")]
    public void Read_Should_Reject_An_Undefined_Value_In_Data_Mode_Without_A_Fallback_Member(string json)
    {
        Action act = () => Read<JsonPlainStatus>(json, EnumReadMode.Data);

        act.Should().Throw<EnumValueJsonException>();
    }

    [Fact]
    public void Read_Should_Reject_An_Undefined_Value_In_Data_Mode_When_UnknownValue_Is_Reject()
    {
        EnumConventions reject = EnumConventions.Default with { UnknownValue = UnknownEnumValueHandling.Reject };

        Action act = () => Read<JsonOrderStatus>("99", EnumReadMode.Data, reject);

        act.Should().Throw<EnumValueJsonException>();
    }

    public static TheoryData<string, EnumReadMode> Malformed()
    {
        TheoryData<string, EnumReadMode> data = [];
        foreach (EnumReadMode mode in new[] { EnumReadMode.Input, EnumReadMode.Data })
        {
            foreach (string json in new[] { "\"\"", "\" pending\"", "\"1.0\"", "\"0x1\"", "\"+1\"", "\"-0\"", "1.5", "true", "{}" })
                data.Add(json, mode);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Malformed))]
    public void Read_Should_Reject_A_Malformed_Value_In_Both_Modes(string json, EnumReadMode mode)
    {
        Action act = () => Read<JsonOrderStatus>(json, mode);

        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData(EnumReadMode.Input)]
    [InlineData(EnumReadMode.Data)]
    public void Read_Should_Return_Null_For_A_Nullable_Enum(EnumReadMode mode)
    {
        JsonOrderStatus? value = Read<JsonOrderStatus?>("null", mode);

        value.Should().BeNull();
    }

    [Theory]
    [InlineData(EnumReadMode.Input)]
    [InlineData(EnumReadMode.Data)]
    public void Read_Should_Read_A_Value_Into_A_Nullable_Enum(EnumReadMode mode)
    {
        JsonOrderStatus? value = Read<JsonOrderStatus?>("\"pending_approval\"", mode);

        value.Should().Be(JsonOrderStatus.PendingApproval);
    }

    [Fact]
    public void Read_Should_Reject_Null_For_A_Non_Nullable_Enum()
    {
        Action act = () => Read<JsonOrderStatus>("null", EnumReadMode.Data);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_Should_Report_The_Json_Path_And_The_Allowed_Values()
    {
        Action act = () => Read<JsonOrder>("{\"Status\":\"bogus\"}", EnumReadMode.Input);

        EnumValueError error = act.Should().Throw<EnumValueJsonException>().Which.Error;
        error.Path.Should().Be("$.Status");
        error.Value.Should().Be("bogus");
        error.EnumType.Should().Be<JsonOrderStatus>();
        error.AllowedValues.Should().Equal("pending", "pending_approval");
    }

    [Fact]
    public void Read_Should_Put_The_Allowed_Values_In_The_Message()
    {
        Action act = () => Read<JsonOrderStatus>("\"bogus\"", EnumReadMode.Input);

        act.Should().Throw<EnumValueJsonException>()
            .WithMessage("'bogus' is not a valid JsonOrderStatus. Allowed values: pending, pending_approval.");
    }

    [Fact]
    public void Write_Should_Write_The_Wire_Name()
    {
        string json = JsonSerializer.Serialize(JsonOrderStatus.PendingApproval, Options());

        json.Should().Be("\"pending_approval\"");
    }

    [Fact]
    public void Write_Should_Write_The_Number_When_WriteAs_Is_Number()
    {
        string json = JsonSerializer.Serialize(JsonOrderStatus.PendingApproval, Options(writeAs: EnumWireFormat.Number));

        json.Should().Be("1");
    }

    [Fact]
    public void Write_Should_Use_The_WriteAs_Of_The_Conventions_When_No_Format_Is_Given()
    {
        EnumConventions numbers = EnumConventions.Default with { WriteAs = EnumWireFormat.Number };

        string json = JsonSerializer.Serialize(JsonOrderStatus.PendingApproval, Options(conventions: numbers));

        json.Should().Be("1");
    }

    [Fact]
    public void Write_Should_Write_Negative_And_Large_Numbers()
    {
        JsonSerializerOptions numbers = Options(writeAs: EnumWireFormat.Number);

        string min = JsonSerializer.Serialize(FormatterLong.MinValue, numbers);
        string max = JsonSerializer.Serialize(FormatterULong.MaxValue, numbers);

        min.Should().Be("-9223372036854775808");
        max.Should().Be("18446744073709551615");
    }

    [Fact]
    public void Read_Should_Read_Negative_And_Large_Numbers()
    {
        FormatterLong min = Read<FormatterLong>("-9223372036854775808", EnumReadMode.Input);
        FormatterULong max = Read<FormatterULong>("18446744073709551615", EnumReadMode.Input);

        min.Should().Be(FormatterLong.MinValue);
        max.Should().Be(FormatterULong.MaxValue);
    }

    [Theory]
    [InlineData(EnumWireFormat.String)]
    [InlineData(EnumWireFormat.Number)]
    public void Write_Should_Throw_For_An_Undefined_Value(EnumWireFormat format)
    {
        Action act = () => JsonSerializer.Serialize((JsonOrderStatus)42, Options(writeAs: format));

        act.Should().Throw<EnumValueException>().Which.Error.Value.Should().Be("42");
    }

    [Fact]
    public void Write_Should_Write_Acronyms_Like_The_Generated_Helper()
    {
        string json = JsonSerializer.Serialize(JsonAcronymKind.HTTPStatus, Options());

        json.Should().Be("\"http_status\"").And.Be($"\"{JsonAcronymKind.HTTPStatus.ToWireName()}\"");
    }

    [Theory]
    [InlineData(EnumWireFormat.String, "{\"pending_approval\":1}")]
    [InlineData(EnumWireFormat.Number, "{\"1\":1}")]
    public void Write_Should_Write_Dictionary_Keys(EnumWireFormat format, string expected)
    {
        Dictionary<JsonOrderStatus, int> map = new() { [JsonOrderStatus.PendingApproval] = 1 };

        string json = JsonSerializer.Serialize(map, Options(writeAs: format));

        json.Should().Be(expected);
    }

    [Theory]
    [InlineData("{\"pending_approval\":1}")]
    [InlineData("{\"Approval\":1}")]
    [InlineData("{\"1\":1}")]
    public void Read_Should_Read_Dictionary_Keys(string json)
    {
        Dictionary<JsonOrderStatus, int>? map = Read<Dictionary<JsonOrderStatus, int>>(json, EnumReadMode.Input);

        map.Should().Equal(new Dictionary<JsonOrderStatus, int> { [JsonOrderStatus.PendingApproval] = 1 });
    }

    [Fact]
    public void Read_Should_Reject_An_Undefined_Dictionary_Key_In_Input_Mode()
    {
        Action act = () => Read<Dictionary<JsonOrderStatus, int>>("{\"99\":1}", EnumReadMode.Input);

        act.Should().Throw<EnumValueJsonException>();
    }

    [Fact]
    public void Write_Should_Write_Collections_As_Arrays_Of_Wire_Names()
    {
        List<JsonOrderStatus?> values = [JsonOrderStatus.Pending, null, JsonOrderStatus.PendingApproval];

        string json = JsonSerializer.Serialize(values, Options());

        json.Should().Be("[\"pending\",null,\"pending_approval\"]");
    }

    [Theory]
    [InlineData(EnumReadMode.Input)]
    [InlineData(EnumReadMode.Data)]
    public void Read_Should_Read_Collections(EnumReadMode mode)
    {
        HashSet<JsonOrderStatus>? set = Read<HashSet<JsonOrderStatus>>("[\"pending\",\"Approval\",1]", mode);
        JsonOrderStatus?[]? array = Read<JsonOrderStatus?[]>("[null,\"pending\"]", mode);

        set.Should().BeEquivalentTo([JsonOrderStatus.Pending, JsonOrderStatus.PendingApproval]);
        array.Should().Equal(null, JsonOrderStatus.Pending);
    }

    [Theory]
    [InlineData(JsonPermissions.ReadWrite, "[\"read\",\"write\"]")]
    [InlineData(JsonPermissions.Read | JsonPermissions.Delete, "[\"read\",\"delete\"]")]
    [InlineData(JsonPermissions.None, "[]")]
    public void Write_Should_Write_Flags_As_An_Array_Of_Single_Flags(JsonPermissions value, string expected)
    {
        string json = JsonSerializer.Serialize(value, Options());

        json.Should().Be(expected);
    }

    [Fact]
    public void Write_Should_Write_Flags_As_A_Number_When_WriteAs_Is_Number()
    {
        string json = JsonSerializer.Serialize(JsonPermissions.ReadWrite | JsonPermissions.Delete, Options(writeAs: EnumWireFormat.Number));

        json.Should().Be("7");
    }

    [Fact]
    public void Write_Should_Throw_For_An_Undefined_Flag()
    {
        Action act = () => JsonSerializer.Serialize((JsonPermissions)8, Options());

        act.Should().Throw<EnumValueException>();
    }

    public static TheoryData<string, EnumReadMode, JsonPermissions> FlagsInputs()
    {
        TheoryData<string, EnumReadMode, JsonPermissions> data = [];
        foreach (EnumReadMode mode in new[] { EnumReadMode.Input, EnumReadMode.Data })
        {
            data.Add("[\"read\",\"write\"]", mode, JsonPermissions.ReadWrite);
            data.Add("[\"READ\",4]", mode, JsonPermissions.Read | JsonPermissions.Delete);
            data.Add("[\"read_write\"]", mode, JsonPermissions.ReadWrite);
            data.Add("[]", mode, JsonPermissions.None);
            data.Add("\"read, write\"", mode, JsonPermissions.ReadWrite);
            data.Add("\"read,delete\"", mode, JsonPermissions.Read | JsonPermissions.Delete);
            data.Add("\"write\"", mode, JsonPermissions.Write);
            data.Add("3", mode, JsonPermissions.ReadWrite);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(FlagsInputs))]
    public void Read_Should_Read_Flags_Arrays_Legacy_Strings_And_Numbers(string json, EnumReadMode mode, JsonPermissions expected)
    {
        JsonPermissions value = Read<JsonPermissions>(json, mode);

        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("8")]
    [InlineData("[\"read\",\"bogus\"]")]
    [InlineData("[\"read\",8]")]
    [InlineData("\"read,,write\"")]
    [InlineData("\"read, bogus\"")]
    [InlineData("[[\"read\"]]")]
    public void Read_Should_Reject_Invalid_Flags(string json)
    {
        Action act = () => Read<JsonPermissions>(json, EnumReadMode.Input);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Flags_Dictionary_Keys_Should_Round_Trip()
    {
        Dictionary<JsonPermissions, int> map = new() { [JsonPermissions.ReadWrite] = 1 };

        string json = JsonSerializer.Serialize(map, Options());
        Dictionary<JsonPermissions, int>? back = JsonSerializer.Deserialize<Dictionary<JsonPermissions, int>>(json, Options());

        json.Should().Be("{\"read,write\":1}");
        back.Should().Equal(map);
    }

    [Fact]
    public void Read_Should_Reject_A_Value_Longer_Than_Every_Spelling_With_A_Truncated_Message()
    {
        string json = "\"" + new string('x', 100_000) + "\"";

        Action act = () => Read<JsonOrderStatus>(json, EnumReadMode.Input);

        EnumValueJsonException exception = act.Should().Throw<EnumValueJsonException>().Which;
        exception.Error.Value.Should().Be(new string('x', 64) + "…");
        exception.Message.Length.Should().BeLessThan(200);
    }

    [Fact]
    public void Read_Should_Reject_A_Long_Escaped_Value()
    {
        string json = "\"" + string.Concat(Enumerable.Repeat("\\u0078", 1_000)) + "\"";

        Action act = () => Read<JsonOrderStatus>(json, EnumReadMode.Input);

        act.Should().Throw<EnumValueJsonException>().Which.Error.Value.Should().StartWith("\\u0078").And.EndWith("…");
    }

    [Fact]
    public void Read_Should_Reject_A_Value_Just_Longer_Than_Every_Spelling()
    {
        string value = new('x', 21);

        Action act = () => Read<JsonOrderStatus>("\"" + value + "\"", EnumReadMode.Input);

        act.Should().Throw<EnumValueJsonException>().Which.Error.Value.Should().Be(value);
    }

    [Fact]
    public void Read_Should_Read_A_Long_Value_As_The_Fallback_Member_In_Data_Mode()
    {
        JsonOrderStatus value = Read<JsonOrderStatus>("\"" + new string('x', 10_000) + "\"", EnumReadMode.Data);

        value.Should().Be(JsonOrderStatus.Unknown);
    }

    [Theory]
    [InlineData("\"\\u0070ending_approval\"")]
    [InlineData("\"\\u0041pproval\"")]
    public void Read_Should_Unescape_A_Value_Before_The_Lookup(string json)
    {
        JsonOrderStatus value = Read<JsonOrderStatus>(json, EnumReadMode.Input);

        value.Should().Be(JsonOrderStatus.PendingApproval);
    }

    [Fact]
    public async Task Read_Should_Handle_Values_Split_Across_Buffers()
    {
        JsonSerializerOptions options = new JsonSerializerOptions { DefaultBufferSize = 1 }
            .AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);
        string valid = "{\"Status\":\"pending_approval\",\"Permissions\":\"read, write\"}";
        string tooLong = "{\"Status\":\"" + new string('x', 5_000) + "\"}";

        JsonOrder? order = await JsonSerializer.DeserializeAsync<JsonOrder>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(valid)), options);
        Func<Task> act = async () =>
            await JsonSerializer.DeserializeAsync<JsonOrder>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tooLong)), options);

        order!.Status.Should().Be(JsonOrderStatus.PendingApproval);
        order.Permissions.Should().Be(JsonPermissions.ReadWrite);
        (await act.Should().ThrowAsync<EnumValueJsonException>()).Which.Error.Value.Should().Be(new string('x', 64) + "…");
    }

    [Fact]
    public void Read_Should_Reject_A_Long_Dictionary_Key()
    {
        string json = "{\"" + new string('x', 10_000) + "\":1}";

        Action act = () => JsonSerializer.Deserialize<Dictionary<JsonOrderStatus, int>>(json, Options(EnumReadMode.Input));

        act.Should().Throw<EnumValueJsonException>();
    }

    [Fact]
    public void Errors_Should_Truncate_Long_Values()
    {
        EnumInfo<JsonOrderStatus> info = EnumMetadata.Get<JsonOrderStatus>();

        bool ok = info.TryParse(new string('z', 100), EnumReadMode.Input, EnumConventions.Default, out _, out EnumValueError? error);

        ok.Should().BeFalse();
        error!.Value.Should().Be(new string('z', 64) + "…");
        info.CreateError(new string('z', 65), EnumReadMode.Data).Value.Should().Be(new string('z', 64) + "…");
        info.CreateError(new string('z', 64), EnumReadMode.Data).Value.Should().Be(new string('z', 64));
    }

    [Fact]
    public void Enums_Rejected_By_CanHandle_Should_Use_The_Default_Serializer()
    {
        string json = JsonSerializer.Serialize(DayOfWeek.Monday, Options());

        json.Should().Be("1");
    }

    [Fact]
    public void Enums_Without_Generated_Metadata_Should_Not_Be_Handled_Without_The_Reflection_Opt_In()
    {
        EnumConventions all = EnumConventions.Default with { CanHandle = static type => type.IsEnum };
        EnumConverterFactory factory = new(all);

        string json = JsonSerializer.Serialize(DayOfWeek.Monday, Options(conventions: all));

        factory.UsesReflectionFallback.Should().BeFalse();
        factory.CanConvert(typeof(DayOfWeek)).Should().BeFalse();
        json.Should().Be("1");
    }

    [Fact]
    public void Enums_Without_Generated_Metadata_Accepted_By_CanHandle_Should_Use_Reflection_Metadata_When_Opted_In()
    {
        EnumConventions all = EnumConventions.Default with { CanHandle = static type => type.IsEnum };
        JsonSerializerOptions options = new JsonSerializerOptions().AddEnumConventionsWithReflection(all, EnumReadMode.Input);

        string json = JsonSerializer.Serialize(DayOfWeek.Monday, options);
        DayOfWeek value = JsonSerializer.Deserialize<DayOfWeek>("\"Monday\"", options);

        EnumConverterFactory.CreateWithReflectionFallback(all).UsesReflectionFallback.Should().BeTrue();
        json.Should().Be("\"monday\"");
        value.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void Reflection_Opt_In_Should_Still_Respect_CanHandle()
    {
        EnumConverterFactory factory = EnumConverterFactory.CreateWithReflectionFallback(EnumConventions.Default);

        factory.CanConvert(typeof(DayOfWeek)).Should().BeFalse();
        factory.CanConvert(typeof(JsonOrderStatus)).Should().BeTrue();
    }

    [Fact]
    public void StringEnum_Without_Generated_Metadata_Should_Fail_Loud_Instead_Of_Writing_A_Number()
    {
        EnumConverterFactory factory = new(EnumConventions.Default);

        Action create = () => factory.CreateConverter(typeof(UnreachableHolder<int>.Status), new JsonSerializerOptions());
        Action serialize = () => JsonSerializer.Serialize(UnreachableHolder<int>.Status.First, Options());
        Action deserialize = () => JsonSerializer.Deserialize<UnreachableHolder<int>.Status>("0", Options());

        EnumMetadata.TryGet(typeof(UnreachableHolder<int>.Status), out _).Should().BeFalse();
        factory.CanConvert(typeof(UnreachableHolder<int>.Status)).Should().BeTrue();
        create.Should().Throw<InvalidOperationException>().WithMessage("*UnreachableHolder*Status*[StringEnum]*no generated metadata*");
        serialize.Should().Throw<InvalidOperationException>().WithMessage("*UnreachableHolder*Status*");
        deserialize.Should().Throw<InvalidOperationException>().WithMessage("*UnreachableHolder*Status*");
    }

    [Fact]
    public void StringEnum_Without_Generated_Metadata_Should_Use_The_Reflection_Opt_In()
    {
        JsonSerializerOptions options = new JsonSerializerOptions().AddEnumConventionsWithReflection(EnumConventions.Default with
        {
            CanHandle = static type => type == typeof(UnreachableHolder<int>.Status),
        });

        JsonSerializer.Serialize(UnreachableHolder<int>.Status.SecondValue, options).Should().Be("\"second_value\"");
    }

    [Fact]
    public void Enums_From_A_Type_Only_Assembly_Should_Use_The_Naming_Of_That_Assembly()
    {
        string json = JsonSerializer.Serialize(ContractStatus.HTTPShipped, Options());

        json.Should().Be("\"http-shipped\"");
    }
}

[StringEnum]
public enum JsonOrderStatus
{
    Pending = 0,
    [EnumAlias("Approval")]
    PendingApproval = 1,
    [EnumFallback]
    Unknown = 99_999,
}

[StringEnum]
public enum JsonPlainStatus
{
    Pending = 0,
    PendingApproval = 1,
}

[StringEnum, Flags]
public enum JsonPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    ReadWrite = Read | Write,
}

[StringEnum]
public enum JsonAcronymKind
{
    HTTPStatus,
}

public sealed class JsonOrder
{
    public JsonOrderStatus Status { get; set; }

    public JsonPermissions Permissions { get; set; }
}
