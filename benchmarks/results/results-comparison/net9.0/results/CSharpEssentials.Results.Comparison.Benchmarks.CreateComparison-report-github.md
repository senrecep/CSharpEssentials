```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **27.0543 ns** | **0.5273 ns** | **0.4932 ns** |   **1.00** |    **0.03** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   7.3519 ns | 0.0053 ns | 0.0047 ns |   0.27 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   |  20.9398 ns | 0.4227 ns | 0.3954 ns |   0.77 |    0.02 | 0.0052 |      88 B |        0.85 |
| FluentResults              | False   | 129.7067 ns | 0.7518 ns | 0.7032 ns |   4.80 |    0.09 | 0.0205 |     344 B |        3.31 |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.1495 ns** | **0.0010 ns** | **0.0009 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   7.4343 ns | 0.0019 ns | 0.0017 ns |  49.71 |    0.29 |      - |         - |          NA |
| ErrorOr                    | True    |   0.1495 ns | 0.0009 ns | 0.0008 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults              | True    |  37.1353 ns | 0.2705 ns | 0.2398 ns | 248.33 |    2.14 | 0.0067 |     112 B |          NA |
