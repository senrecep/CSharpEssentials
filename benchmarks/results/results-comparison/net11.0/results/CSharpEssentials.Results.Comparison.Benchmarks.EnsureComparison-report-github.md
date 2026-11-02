```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **19.026 ns** | **0.1924 ns** | **0.1800 ns** |  **1.00** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  1.800 ns | 0.0027 ns | 0.0025 ns |  0.09 |      - |         - |        0.00 |
| ErrorOr                    | False   | 16.262 ns | 0.1436 ns | 0.1343 ns |  0.85 | 0.0052 |      88 B |        0.85 |
|                            |         |           |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.421 ns** | **0.0046 ns** | **0.0043 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  2.353 ns | 0.0040 ns | 0.0037 ns |  1.66 |      - |         - |          NA |
| ErrorOr                    | True    |  1.154 ns | 0.0098 ns | 0.0077 ns |  0.81 |      - |         - |          NA |
