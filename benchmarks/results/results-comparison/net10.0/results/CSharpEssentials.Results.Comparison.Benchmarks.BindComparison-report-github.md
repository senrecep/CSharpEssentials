```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **1.2449 ns** | **0.0031 ns** | **0.0026 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | False   |  1.3721 ns | 0.0054 ns | 0.0050 ns |   1.10 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |  1.9763 ns | 0.0061 ns | 0.0051 ns |   1.59 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |  0.9622 ns | 0.0020 ns | 0.0017 ns |   0.77 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 55.1934 ns | 0.3499 ns | 0.3273 ns |  44.34 |    0.27 | 0.0124 |     208 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.7201 ns** | **0.0048 ns** | **0.0040 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | True    |  1.1559 ns | 0.0012 ns | 0.0010 ns |   1.61 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  1.9689 ns | 0.0053 ns | 0.0049 ns |   2.73 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |  0.6052 ns | 0.0042 ns | 0.0040 ns |   0.84 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 72.3925 ns | 0.5176 ns | 0.4842 ns | 100.54 |    0.85 | 0.0191 |     320 B |          NA |
