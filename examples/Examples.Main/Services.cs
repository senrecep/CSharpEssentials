using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace Examples.Main;

public static class Services
{
    public static Maybe<User> FindUserByEmail(string email)
    {
        if (email == "alice@example.com")
            return new User { Email = email, Name = "Alice Smith" }.AsMaybe();
        return Maybe<User>.None;
    }

    public static Result<Order> PlaceOrder(User user, decimal total)
    {
        Order order = new()
        {
            OrderNumber = $"ORD-{Guider.ToStringFromGuid(Guider.NewGuid())[..8].ToUpper()}",
            Total = total,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Success(order);
    }
}
