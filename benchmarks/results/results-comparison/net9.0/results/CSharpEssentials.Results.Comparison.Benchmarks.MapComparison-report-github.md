```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **0.8862 ns** | **0.0032 ns** | **0.0030 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   9.1072 ns | 0.0097 ns | 0.0086 ns |  10.28 |    0.03 |      - |         - |          NA |
| ErrorOr                    | False   |   0.8325 ns | 0.0013 ns | 0.0010 ns |   0.94 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 139.1249 ns | 1.4085 ns | 1.1761 ns | 157.00 |    1.38 | 0.0205 |     344 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.8496 ns** | **0.0011 ns** | **0.0011 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   8.9063 ns | 0.0023 ns | 0.0022 ns |  10.48 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   0.8490 ns | 0.0014 ns | 0.0012 ns |   1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 103.4457 ns | 0.9481 ns | 0.8405 ns | 121.76 |    0.97 | 0.0153 |     256 B |          NA |
