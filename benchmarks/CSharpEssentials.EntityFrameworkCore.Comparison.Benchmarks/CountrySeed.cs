namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>One row of the seed data <c>MigrateDataAsync</c> syncs the countries table to.</summary>
public sealed record CountrySeed(string Code, string Name);
