```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                        | Categories   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Plain        | Plain        |  5.310 ns | 0.0038 ns | 0.0033 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| ErrorOr_Plain                 | Plain        |  5.576 ns | 0.0014 ns | 0.0012 ns |  1.05 |    0.00 |      - |      - |         - |          NA |
| FluentResults_Plain           | Plain        | 26.869 ns | 0.3739 ns | 0.3498 ns |  5.06 |    0.06 | 0.0091 | 0.0001 |     152 B |          NA |
|                               |              |           |           |           |       |         |        |        |           |             |
| CSharpEssentials_WithMetadata | WithMetadata | 62.005 ns | 0.6800 ns | 0.6361 ns |  1.00 |    0.01 | 0.0129 |      - |     216 B |        1.00 |
| ErrorOr_WithMetadata          | WithMetadata | 59.101 ns | 0.4061 ns | 0.3799 ns |  0.95 |    0.01 | 0.0129 |      - |     216 B |        1.00 |
| FluentResults_WithMetadata    | WithMetadata | 67.941 ns | 0.3686 ns | 0.3448 ns |  1.10 |    0.01 | 0.0172 |      - |     288 B |        1.33 |
