namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>How the three <see cref="CreateCustomer"/> validators are ordered.</summary>
public enum ValidatorGroups
{
    /// <summary>Name, Email and Age validators, all with Order 0.</summary>
    One,

    /// <summary>Name and Email validators with Order 0, then the Age validator with Order 1.</summary>
    Two,
}
