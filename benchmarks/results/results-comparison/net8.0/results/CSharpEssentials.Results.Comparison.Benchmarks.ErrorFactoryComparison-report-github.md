```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                        | Categories   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Plain        | Plain        | 10.269 ns | 0.0039 ns | 0.0035 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| ErrorOr_Plain                 | Plain        |  6.711 ns | 0.0009 ns | 0.0007 ns |  0.65 |    0.00 |      - |      - |         - |          NA |
| FluentResults_Plain           | Plain        | 36.880 ns | 0.2033 ns | 0.1802 ns |  3.59 |    0.02 | 0.0091 | 0.0001 |     152 B |          NA |
|                               |              |           |           |           |       |         |        |        |           |             |
| CSharpEssentials_WithMetadata | WithMetadata | 82.096 ns | 0.2487 ns | 0.2205 ns |  1.00 |    0.00 | 0.0129 |      - |     216 B |        1.00 |
| ErrorOr_WithMetadata          | WithMetadata | 81.072 ns | 0.3503 ns | 0.3277 ns |  0.99 |    0.00 | 0.0129 |      - |     216 B |        1.00 |
| FluentResults_WithMetadata    | WithMetadata | 89.657 ns | 0.3643 ns | 0.3229 ns |  1.09 |    0.00 | 0.0172 |      - |     288 B |        1.33 |
