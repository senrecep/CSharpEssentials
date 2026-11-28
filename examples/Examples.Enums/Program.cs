using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

Console.WriteLine("--- Wire names ---");
foreach (OrderStatus status in OrderStatusExtensions.Values)
    Console.WriteLine($"{status,-16} wire={status.ToWireName(),-18} description={status.ToDescription() ?? "(none)"}");
Console.WriteLine($"const PendingApprovalWireName = {OrderStatusExtensions.PendingApprovalWireName}");
Console.WriteLine($"kebab-case enum: {DeliverySpeed.NextDay.ToWireName()}");

Console.WriteLine("\n--- Parsing with the generated helpers ---");
foreach (string text in new[] { "pending_approval", "PendingApproval", "approval", "sent", "1", "gone" })
{
    bool ok = OrderStatusExtensions.TryParseWire(text, out OrderStatus value);
    Console.WriteLine($"TryParseWire(\"{text}\") = {ok}{(ok ? $" -> {value}" : "")}");
}
try
{
    OrderStatusExtensions.ParseWire("gone");
}
catch (EnumValueException ex)
{
    Console.WriteLine($"ParseWire(\"gone\") throws: {ex.Message}");
}

Console.WriteLine("\n--- Input and Data read modes ---");
EnumConventions conventions = EnumConventions.Default;
if (!EnumValueParser.TryParse("gone", EnumReadMode.Input, conventions, out OrderStatus _, out EnumValueError? error))
    Console.WriteLine($"Input: rejected, allowed values: {string.Join(", ", error.AllowedValues)}");
EnumValueParser.TryParse("gone", EnumReadMode.Data, conventions, out OrderStatus fromDatabase);
Console.WriteLine($"Data:  mapped to the fallback member {fromDatabase}");
EnumConventions noNumbers = conventions with { AcceptNumbers = false };
Console.WriteLine($"Input without numbers accepts \"1\": {EnumValueParser.TryParse("1", EnumReadMode.Input, noNumbers, out OrderStatus _)}");

Console.WriteLine("\n--- Other generated helpers ---");
OrderStatus shipped = OrderStatus.Shipped;
Console.WriteLine($"ToOptimizedString={shipped.ToOptimizedString()} ToKebabCase={shipped.ToKebabCase()} ToLowerCase={shipped.ToLowerCase()} ToUpperCase={shipped.ToUpperCase()}");
Console.WriteLine($"AsUnderlyingType={shipped.AsUnderlyingType()} IsDefined={shipped.IsDefined()} ((OrderStatus)42).IsDefined={((OrderStatus)42).IsDefined()}");
Console.WriteLine($"IsDefined(\"Shipped\")={OrderStatusExtensions.IsDefined("Shipped")} Names=[{string.Join(", ", OrderStatusExtensions.Names)}] GetValues().Length={OrderStatusExtensions.GetValues().Length}");

Console.WriteLine("\n--- Metadata and formatter ---");
EnumInfo<OrderStatus> info = EnumMetadata.Get<OrderStatus>();
Console.WriteLine($"WireNames=[{string.Join(", ", info.WireNames)}] Fallback={info.Fallback?.MemberName}");
foreach (EnumMemberInfo<OrderStatus> member in info.TypedMembers)
    Console.WriteLine($"  {member.MemberName}: aliases=[{string.Join(", ", member.Aliases)}] fallback={member.IsFallback}");
Console.WriteLine($"Format(String)={EnumValueFormatter.Format(shipped, EnumWireFormat.String)} Format(Number)={EnumValueFormatter.Format(shipped, EnumWireFormat.Number)}");
Console.WriteLine($"IsRegistered={EnumMetadata.IsRegistered(typeof(OrderStatus))}");

Console.WriteLine("\n--- Flags ---");
Permissions granted = Permissions.Read | Permissions.Write;
Console.WriteLine($"ToWireName={granted.ToWireName()} ParseWire(\"5\")={PermissionsExtensions.ParseWire("5")} IsDefined(8)={((Permissions)8).IsDefined()}");

Console.WriteLine("\n--- JSON with CSharpEssentials.Json ---");
JsonSerializerOptions json = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default);
string payload = JsonSerializer.Serialize(new Order(OrderStatus.PendingApproval, granted), json);
Console.WriteLine(payload);
Order? read = JsonSerializer.Deserialize<Order>("""{"status":"approval","permissions":["delete"]}""", json);
Console.WriteLine($"read back: {read!.Status}, {read.Permissions}");

[StringEnum]
public enum OrderStatus
{
    Pending,
    [EnumAlias("approval")][Description("Waiting for a manager")] PendingApproval,
    [JsonStringEnumMemberName("sent")] Shipped,
    [EnumFallback] Unknown = 99,
}

[StringEnum(Naming = EnumNaming.KebabCaseLower)]
public enum DeliverySpeed { Standard, NextDay }

[StringEnum]
[Flags]
public enum Permissions { None = 0, Read = 1, Write = 2, Delete = 4 }

public sealed record Order(OrderStatus Status, Permissions Permissions);
