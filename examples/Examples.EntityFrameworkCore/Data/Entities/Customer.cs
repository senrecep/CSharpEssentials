using CSharpEssentials.Entity;
using CSharpEssentials.Maybe;

namespace Examples.EntityFrameworkCore.Data;

public sealed class Customer : EntityBase<Guid>
{
    private Customer() { }

    public string Email { get; private set; } = "";
    public Maybe<string>? Nickname { get; private set; }

    public static Customer Create(string email, Maybe<string> nickname = default) =>
        new() { Id = Guid.NewGuid(), Email = email, Nickname = nickname };
}
