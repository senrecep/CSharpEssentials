using CSharpEssentials.Clone;

Console.WriteLine("--- ICloneable<T> on one object ---");
Person original = new() { Name = "Alice", Address = new Address { City = "New York" }, Tags = ["admin"] };
Person copy = original.Clone();
copy.Name = "Bob";
copy.Address.City = "Los Angeles";
copy.Tags.Add("beta");
Console.WriteLine($"original: {original.Name}, {original.Address.City}, [{string.Join(",", original.Tags)}]");
Console.WriteLine($"copy:     {copy.Name}, {copy.Address.City}, [{string.Join(",", copy.Tags)}]");
Console.WriteLine($"same address instance: {ReferenceEquals(original.Address, copy.Address)}");

Console.WriteLine("\n--- Clone() on IEnumerable<T> ---");
List<Person> people = [original, new() { Name = "Carol", Address = new Address { City = "Paris" } }];
int calls = 0;
IEnumerable<Person> lazy = people.Select(p => { calls++; return p; }).Clone();
Console.WriteLine($"items cloned before enumeration: {calls}");
List<Person> copies = lazy.ToList();
Console.WriteLine($"items cloned after ToList: {calls}");
copies[1].Address.City = "Lyon";
Console.WriteLine($"original city: {people[1].Address.City}, copy city: {copies[1].Address.City}");

Console.WriteLine("\n--- Arrays ---");
Address[] addresses = [new() { City = "Rome" }];
object shallow = addresses.Clone();
Address[] deep = ((IEnumerable<Address>)addresses).Clone().ToArray();
Console.WriteLine($"Array.Clone shares items: {ReferenceEquals(((Address[])shallow)[0], addresses[0])}");
Console.WriteLine($"Clone extension copies items: {ReferenceEquals(deep[0], addresses[0])}");

Console.WriteLine("\n--- Clone() on IQueryable<T> ---");
IQueryable<Person> query = people.AsQueryable().Where(p => p.Name.StartsWith('C'));
IQueryable<Person> cloned = query.Clone();
Console.WriteLine($"cloned query result: {string.Join(",", cloned.Select(p => p.Name))} ({cloned.GetType().Name})");

public sealed class Person : ICloneable<Person>
{
    public string Name { get; set; } = "";
    public Address Address { get; set; } = new();
    public List<string> Tags { get; set; } = [];

    public Person Clone() => new() { Name = Name, Address = Address.Clone(), Tags = [.. Tags] };
}

public sealed class Address : ICloneable<Address>
{
    public string City { get; set; } = "";

    public Address Clone() => new() { City = City };
}
