```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.4947 ns** | **0.0034 ns** | **0.0032 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.0904 ns | 0.0058 ns | 0.0054 ns |  2.20 |    0.02 |         - |          NA |
| ErrorOr                    | False   | 0.5247 ns | 0.0021 ns | 0.0020 ns |  1.06 |    0.01 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.5815 ns** | **0.0155 ns** | **0.0145 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.7241 ns | 0.0167 ns | 0.0148 ns |  1.09 |    0.01 |         - |          NA |
| ErrorOr                    | True    | 1.6022 ns | 0.0116 ns | 0.0108 ns |  1.01 |    0.01 |         - |          NA |
