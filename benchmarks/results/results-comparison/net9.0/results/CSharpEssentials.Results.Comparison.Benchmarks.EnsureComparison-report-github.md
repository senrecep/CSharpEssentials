```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **26.128 ns** | **0.5386 ns** | **0.8225 ns** |  **1.00** |    **0.04** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  8.462 ns | 0.0058 ns | 0.0051 ns |  0.32 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   | 21.307 ns | 0.4185 ns | 0.3915 ns |  0.82 |    0.03 | 0.0052 |      88 B |        0.85 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.155 ns** | **0.0085 ns** | **0.0079 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  8.963 ns | 0.0061 ns | 0.0054 ns |  7.76 |    0.05 |      - |         - |          NA |
| ErrorOr                    | True    |  1.371 ns | 0.0115 ns | 0.0107 ns |  1.19 |    0.01 |      - |         - |          NA |
