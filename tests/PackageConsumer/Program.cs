using CSharpEssentials.Enums;

// ToWireName and the KebabCaseLower spelling exist only when the generator and the buildTransitive props reached this project.
string wireName = OrderStatus.PendingApproval.ToWireName();
if (wireName != "pending-approval")
    throw new InvalidOperationException($"Expected 'pending-approval', got '{wireName}'.");
if (!EnumMetadata.IsRegistered(typeof(OrderStatus)))
    throw new InvalidOperationException("OrderStatus has no registered metadata.");

Console.WriteLine(wireName);

[StringEnum]
internal enum OrderStatus
{
    Pending,
    PendingApproval,
}
