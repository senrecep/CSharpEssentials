using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

public class StringEnumGeneratorTests
{
    [Fact]
    public void ToOptimizedString_Should_Return_Name()
    {
        Color c = Color.Red;
        c.ToOptimizedString().Should().Be("Red");
    }

    [Fact]
    public void ToOptimizedString_Should_Return_Name_For_Last_Member()
    {
        Color c = Color.Blue;
        c.ToOptimizedString().Should().Be("Blue");
    }

    [Fact]
    public void ToOptimizedString_Should_Return_Name_For_CamelCase_Member()
    {
        Status s = Status.InProgress;
        s.ToOptimizedString().Should().Be("InProgress");
    }

    [Fact]
    public void ToSnakeCase_Should_Return_SnakeCase()
    {
        Status s = Status.InProgress;
        s.ToSnakeCase().Should().Be("in_progress");
    }

    [Fact]
    public void ToSnakeCase_Should_Return_SnakeCase_For_Simple_Name()
    {
        Color c = Color.Red;
        c.ToSnakeCase().Should().Be("red");
    }

    [Fact]
    public void ToKebabCase_Should_Return_KebabCase()
    {
        Status s = Status.InProgress;
        s.ToKebabCase().Should().Be("in-progress");
    }

    [Fact]
    public void ToLowerCase_Should_Return_LowerCase()
    {
        Color c = Color.Red;
        c.ToLowerCase().Should().Be("red");
    }

    [Fact]
    public void ToUpperCase_Should_Return_UpperCase()
    {
        Color c = Color.Red;
        c.ToUpperCase().Should().Be("RED");
    }

    [Fact]
    public void IsDefined_Should_Return_True_For_Known()
    {
        ColorExtensions.IsDefined("Green").Should().BeTrue();
    }

    [Fact]
    public void IsDefined_Should_Return_False_For_Unknown()
    {
        ColorExtensions.IsDefined("Purple").Should().BeFalse();
    }

    [Fact]
    public void TryParse_Should_Return_True_And_Value_For_Valid_Name()
    {
        bool result = ColorExtensions.TryParse("Green", out Color value);
        result.Should().BeTrue();
        value.Should().Be(Color.Green);
    }

    [Fact]
    public void TryParse_Should_Return_False_For_Invalid_Name()
    {
        bool result = ColorExtensions.TryParse("Purple", out Color value);
        result.Should().BeFalse();
        value.Should().Be(Color.Red); // default(int) cast to enum = first value (0)
    }

    [Fact]
    public void TryParse_Should_Parse_Numeric_Value()
    {
        bool result = ColorExtensions.TryParse("2", out Color value);
        result.Should().BeTrue();
        value.Should().Be(Color.Blue);
    }

    [Fact]
    public void Parse_Should_Return_Value_For_Valid_Name()
    {
        Color value = ColorExtensions.Parse("Green");
        value.Should().Be(Color.Green);
    }

    [Fact]
    public void Parse_Should_Throw_For_Invalid_Name()
    {
        Action act = () => ColorExtensions.Parse("Purple");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetNames_Should_Return_All_Names()
    {
        string[] names = ColorExtensions.GetNames();
        names.Should().ContainInOrder("Red", "Green", "Blue");
    }

    [Fact]
    public void GetValues_Should_Return_All_Values()
    {
        Color[] values = ColorExtensions.GetValues();
        values.Should().ContainInOrder(Color.Red, Color.Green, Color.Blue);
    }

    [Fact]
    public void AsUnderlyingType_Should_Return_Numeric_Value()
    {
        Color c = Color.Blue;
        c.AsUnderlyingType().Should().Be(2);
    }

    [Fact]
    public void Constants_Should_Have_Correct_SnakeCase()
    {
        StatusExtensions.InProgressSnakeCase.Should().Be("in_progress");
        StatusExtensions.NotStartedSnakeCase.Should().Be("not_started");
    }

    [Fact]
    public void Constants_Should_Have_Correct_KebabCase()
    {
        StatusExtensions.InProgressKebabCase.Should().Be("in-progress");
        StatusExtensions.NotStartedKebabCase.Should().Be("not-started");
    }

    [Fact]
    public void ToSnakeCase_Should_Handle_Consecutive_Uppercase()
    {
        HttpStatus h = HttpStatus.HTTPResponse;
        h.ToSnakeCase().Should().Be("httpresponse");
    }

    [Fact]
    public void ToKebabCase_Should_Handle_Consecutive_Uppercase()
    {
        HttpStatus h = HttpStatus.HTTPResponse;
        h.ToKebabCase().Should().Be("httpresponse");
    }

    [Fact]
    public void Fallback_Should_Handle_Unknown_Value()
    {
        HttpStatus unknown = (HttpStatus)999;
        unknown.ToSnakeCase().Should().Be("999");
        unknown.ToKebabCase().Should().Be("999");
    }

    [Fact]
    public void Constants_Should_Handle_Consecutive_Uppercase()
    {
        HttpStatusExtensions.OKSnakeCase.Should().Be("ok");
        HttpStatusExtensions.NotFoundSnakeCase.Should().Be("not_found");
        HttpStatusExtensions.HTTPResponseSnakeCase.Should().Be("httpresponse");
        HttpStatusExtensions.HTTPResponseKebabCase.Should().Be("httpresponse");
    }
}

[StringEnum]
internal enum Color
{
    Red,
    Green,
    Blue
}

[StringEnum]
internal enum Status
{
    NotStarted,
    InProgress,
    Completed
}

[StringEnum]
internal enum HttpStatus
{
    OK,
    NotFound,
    HTTPResponse
}
