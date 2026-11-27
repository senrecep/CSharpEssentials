namespace CSharpEssentials.Results.Typed.Tests.Typed;

internal sealed class UserDto(int id)
{
    public int Id { get; } = id;
}
