using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>The conventions and model of one <see cref="StoredOrderContext"/> setup; <see cref="Key"/> must be unique per setup.</summary>
public sealed record StoredOrderModel(string Key, Action<ModelConfigurationBuilder>? Conventions, Action<ModelBuilder> Model);
