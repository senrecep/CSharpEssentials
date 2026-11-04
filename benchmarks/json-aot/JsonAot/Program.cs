using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Json;
using JsonAotCore;
using JsonAotEvents;

namespace JsonAot;

public static class Program
{
    public static int Main()
    {
        JsonSerializerOptions options = new JsonSerializerOptions(AppJsonContext.Default.Options).AddPolymorphism();
        var events = (JsonTypeInfo<IEvent>)options.GetTypeInfo(typeof(IEvent));
        var holders = (JsonTypeInfo<Data>)options.GetTypeInfo(typeof(Data));
        Guid id = Guid.Parse("8d4f1c7e-35f0-4a51-9a35-6f0a3c6b1d22");

        string created = JsonSerializer.Serialize(new UserCreated(id, "Ada"), events);
        IEvent? back = JsonSerializer.Deserialize($$"""{"id":"{{id}}","$event":"user.deleted.v1"}""", events);
        string data = JsonSerializer.Serialize(new Data { Items = [new TextValue("a"), new NumberValue(2)] }, holders);
        Data? dataBack = JsonSerializer.Deserialize(data, holders);

        bool ok = created == $$"""{"$event":"user.created.v1","id":"{{id}}","name":"Ada"}""" &&
            Equals(back, new UserDeleted(id)) &&
            data == """{"items":[{"values":"text","text":"a"},{"values":"number","number":2}]}""" &&
            dataBack?.Items is [TextValue { Text: "a" }, NumberValue { Number: 2 }];
        Console.WriteLine($"{created} {data} {(ok ? "ok" : "MISMATCH")}");
        return ok ? 0 : 1;
    }
}
