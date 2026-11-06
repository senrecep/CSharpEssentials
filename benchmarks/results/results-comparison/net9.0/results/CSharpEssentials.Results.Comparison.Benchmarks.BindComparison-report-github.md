```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **0.9707 ns** | **0.0025 ns** | **0.0020 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   9.0585 ns | 0.0463 ns | 0.0433 ns |   9.33 |    0.05 |      - |         - |          NA |
| ErrorOr                    | False   |   0.9328 ns | 0.0105 ns | 0.0098 ns |   0.96 |    0.01 |      - |         - |          NA |
| FluentResults              | False   |  74.5485 ns | 0.7710 ns | 0.7212 ns |  76.80 |    0.73 | 0.0124 |     208 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.8536 ns** | **0.0023 ns** | **0.0022 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   9.2209 ns | 0.0013 ns | 0.0011 ns |  10.80 |    0.03 |      - |         - |          NA |
| ErrorOr                    | True    |   0.8515 ns | 0.0009 ns | 0.0007 ns |   1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 121.2580 ns | 0.6765 ns | 0.6328 ns | 142.05 |    0.80 | 0.0191 |     320 B |          NA |
