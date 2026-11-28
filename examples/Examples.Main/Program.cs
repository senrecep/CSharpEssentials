using System.Text.Json;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;
using Examples.Main;

Console.WriteLine("--- Maybe, Result and Error together ---");
foreach (string email in new[] { "alice@example.com", "nobody@example.com" })
{
    Result<Order> placed = Services.FindUserByEmail(email)
        .ToResult(Error.NotFound("User.NotFound", $"No user with email {email}."))
        .Then(user => Services.PlaceOrder(user, 99.99m));
    placed.Switch(
        onSuccess: order => Console.WriteLine($"{email}: order {order.OrderNumber} for {order.Total}"),
        onError: errors => Console.WriteLine($"{email}: {errors[0].Code}"));
}

Result unit = Services.FindUserByEmail("alice@example.com").ToMaybeUnitResult(Error.NotFound("User.NotFound", "User not found"));
Console.WriteLine($"ToMaybeUnitResult: success = {unit.IsSuccess}");

Console.WriteLine("\n--- Core and Time ---");
IDateTimeProvider clock = new DateTimeProvider(TimeProvider.System);
Guid id = Guider.NewGuid(clock.UtcNow);
Console.WriteLine($"GUID stamped with the clock: {id} ({Guider.ToStringFromGuid(id)})");
Console.WriteLine($"Sanitized input: {"  order line item  ".Trim().ToPascalCase()}");

Console.WriteLine("\n--- Results string helpers ---");
Console.WriteLine($"TrimStart(\"Hello\") on HelloWorld: {"HelloWorld".TrimStart("Hello").Match(s => s, e => e[0].Code)}");
Console.WriteLine($"TrimStart(\"World\") on Hello:      {"Hello".TrimStart("World").Match(s => s, e => e[0].Code)}");

Console.WriteLine("\n--- Meta-package JSON helpers ---");
using JsonDocument doc = JsonDocument.Parse("""{"user":{"name":"Alice","email":"alice@example.com"},"userName":"alice1"}""");
Console.WriteLine($"TryGetProperty(\"user_name\", \"userName\"): {doc.RootElement.TryGetProperty("user_name", "userName").Match(e => e.GetString(), e => e[0].Code)}");
Console.WriteLine($"TryGetNestedProperty(\"user\", \"name\"):    {doc.TryGetNestedProperty("user", "name").Match(e => e?.GetString(), e => e[0].Code)}");
Console.WriteLine($"TryGetProperty(\"missing\"):              {doc.RootElement.TryGetProperty("missing").Match(e => e.ToString(), e => e[0].Code)}");
Console.WriteLine($"TryGetProperty() with no names:         {doc.RootElement.TryGetProperty().Match(e => e.ToString(), e => e[0].Code)}");
JsonDocument? noDocument = null;
Console.WriteLine($"TryGetNestedProperty on null:           {noDocument.TryGetNestedProperty("a").Match(e => e.ToString(), e => e[0].Code)}");

Console.WriteLine("\n--- Meta-package time helper ---");
long? timestamp = 1_700_000_000_000;
long? missing = null;
Console.WriteLine($"MsToDateTime(1700000000000): {timestamp.MsToDateTime().Match(d => d.ToString("O"), () => "None")}");
Console.WriteLine($"MsToDateTime(null):          {missing.MsToDateTime().Match(d => d.ToString("O"), () => "None")}");
Console.WriteLine($"MsToDateTime(null, epoch):   {missing.MsToDateTime(DateTime.UnixEpoch).Match(d => d.ToString("O"), () => "None")}");
