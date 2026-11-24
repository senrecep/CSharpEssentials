namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class UserDto(int id)
{
    public int Id { get; } = id;
}
