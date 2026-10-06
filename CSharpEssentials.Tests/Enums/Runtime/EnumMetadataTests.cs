using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumMetadataTests
{
    [Fact]
    public void Reflection_Metadata_Should_Read_Member_Attributes()
    {
        IEnumInfo info = EnumMetadata.GetOrCreateWithReflection(typeof(ParserStatus));

        info.EnumType.Should().Be<ParserStatus>();
        info.UnderlyingType.Should().Be<int>();
        info.IsFlags.Should().BeFalse();
        info.Storage.Should().Be(EnumStorage.Default);
        info.WireNames.Should().Equal("pending", "in_progress", "done", "cancelled_by_user", "http_status", "aborted", "unknown");
        info.Fallback!.MemberName.Should().Be(nameof(ParserStatus.Unknown));
        info.Members[0].Description.Should().Be("Waiting for work");
        info.Members[1].Aliases.Should().Equal("Started", "Running");
        info.Members[4].Aliases.Should().Equal("httpstatus");
        info.Members[5].IsObsolete.Should().BeTrue();
        info.Members[6].IsFallback.Should().BeTrue();
        info.Members[6].NumericText.Should().Be("100");
        info.DefinedMask.Should().Be(0 | 1 | 2 | 3 | 4 | 5 | 100);
    }

    [Fact]
    public void Reflection_Metadata_Should_Be_Cached_Per_Naming_And_Never_Registered()
    {
        IEnumInfo snake = EnumMetadata.GetOrCreateWithReflection(typeof(ParserStatus));
        EnumMetadata.GetOrCreateWithReflection(typeof(ParserStatus)).Should().BeSameAs(snake);

        IEnumInfo kebab = EnumMetadata.GetOrCreateWithReflection(typeof(ParserStatus), EnumNaming.KebabCaseLower);
        kebab.WireNames[1].Should().Be("in-progress");
        kebab.WireNames[2].Should().Be("done");

        EnumMetadata.IsRegistered(typeof(ParserStatus)).Should().BeFalse();
        EnumMetadata.TryGet(typeof(ParserStatus), out _).Should().BeFalse();
        EnumMetadata.TryGet<ParserStatus>(out _).Should().BeFalse();
        Action get = () => EnumMetadata.Get<ParserStatus>();
        get.Should().Throw<InvalidOperationException>().WithMessage("*ParserStatus*[StringEnum]*");
    }

    [Fact]
    public void Reflection_Metadata_Should_Reject_Non_Enum_Types()
    {
        Action act = () => EnumMetadata.GetOrCreateWithReflection(typeof(int));
        act.Should().Throw<ArgumentException>();
        EnumMetadata.IsRegistered(typeof(int)).Should().BeFalse();
    }

    [Fact]
    public void Registered_Metadata_Should_Be_Returned_By_Every_Accessor()
    {
        EnumInfo<ManuallyRegistered> info = new(
            [
                new EnumMemberInfo<ManuallyRegistered>(ManuallyRegistered.First, 0, "First", "first"),
                new EnumMemberInfo<ManuallyRegistered>(ManuallyRegistered.SecondValue, 1, "SecondValue", "second-value"),
            ],
            static v => unchecked((ulong)(long)v),
            static r => unchecked((ManuallyRegistered)(long)r),
            isFlags: false,
            EnumStorage.Integer);

        EnumMetadata.Register(info);

        EnumMetadata.Get<ManuallyRegistered>().Should().BeSameAs(info);
        EnumMetadata.TryGet<ManuallyRegistered>(out EnumInfo<ManuallyRegistered>? typed).Should().BeTrue();
        typed.Should().BeSameAs(info);
        EnumMetadata.TryGet(typeof(ManuallyRegistered), out IEnumInfo? untyped).Should().BeTrue();
        untyped.Should().BeSameAs(info);
        EnumMetadata.IsRegistered(typeof(ManuallyRegistered)).Should().BeTrue();
        EnumConventions.Default.CanHandle(typeof(ManuallyRegistered)).Should().BeTrue();
        EnumMetadata.GetOrCreateWithReflection(typeof(ManuallyRegistered)).Should().BeSameAs(info);

        EnumValueFormatter.Format(ManuallyRegistered.SecondValue, EnumWireFormat.String).Should().Be("second-value");
        EnumValueParser.TryParse("Second-Value", EnumReadMode.Input, EnumConventions.Default, out ManuallyRegistered value, out _)
            .Should().BeTrue();
        value.Should().Be(ManuallyRegistered.SecondValue);
        EnumValueParser.TryParseNumber(1L, EnumReadMode.Input, EnumConventions.Default, out value, out _).Should().BeTrue();
        EnumValueParser.TryParseNumber(1UL, EnumReadMode.Input, EnumConventions.Default, out value, out _).Should().BeTrue();
        List<string> names = [];
        EnumValueFormatter.FormatFlags(ManuallyRegistered.First, names);
        names.Should().Equal("first");
    }

    [Fact]
    public void RegisterRange_Should_Build_Metadata_Once_On_First_Lookup()
    {
        int calls = 0;
        EnumInfo<LazilyRegistered> info = new(
            [new EnumMemberInfo<LazilyRegistered>(LazilyRegistered.Only, 0, "Only", "only")],
            static v => unchecked((ulong)(long)v),
            static r => unchecked((LazilyRegistered)(long)r),
            isFlags: false,
            EnumStorage.Integer);

        EnumMetadata.RegisterRange([new(typeof(LazilyRegistered), () =>
        {
            Interlocked.Increment(ref calls);
            return info;
        })]);

        calls.Should().Be(0);
        EnumMetadata.TryGet(typeof(LazilyRegistered), out IEnumInfo? untyped).Should().BeTrue();
        untyped.Should().BeSameAs(info);
        EnumMetadata.Get<LazilyRegistered>().Should().BeSameAs(info);
        calls.Should().Be(1);
    }

    [Fact]
    public void RegisterRange_Should_Reject_Null_Entries()
    {
        Action nullArray = () => EnumMetadata.RegisterRange(null!);
        Action nullFactory = () => EnumMetadata.RegisterRange([new(typeof(LazilyRegistered), null!)]);

        nullArray.Should().Throw<ArgumentNullException>();
        nullFactory.Should().Throw<ArgumentException>().WithMessage("*LazilyRegistered*");
    }

    [Fact]
    public void Visitor_Should_Reach_Typed_Metadata()
    {
        IEnumInfo info = EnumMetadata.GetOrCreateWithReflection(typeof(StrictStatus));
        info.Accept(new TypeNameVisitor()).Should().Be(nameof(StrictStatus));
    }

    [Fact]
    public void Error_Message_Should_Omit_Empty_Allowed_Values()
    {
        EnumValueError error = new(typeof(StrictStatus), "x", [], null);
        error.Message.Should().Be("'x' is not a valid StrictStatus.");
        new EnumValueException(error).Message.Should().Be(error.Message);
    }

    private sealed class TypeNameVisitor : IEnumInfoVisitor<string>
    {
        public string Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum => typeof(TEnum).Name;
    }
}
