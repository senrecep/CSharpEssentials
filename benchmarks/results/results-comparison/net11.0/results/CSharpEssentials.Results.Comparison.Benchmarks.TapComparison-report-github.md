```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.4957 ns** | **0.0088 ns** | **0.0078 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.0796 ns | 0.0129 ns | 0.0107 ns |  2.18 |    0.04 |         - |          NA |
| ErrorOr                    | False   | 0.5241 ns | 0.0020 ns | 0.0017 ns |  1.06 |    0.02 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.5774 ns** | **0.0113 ns** | **0.0106 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.8860 ns | 0.0531 ns | 0.0496 ns |  1.20 |    0.03 |         - |          NA |
| ErrorOr                    | True    | 1.6358 ns | 0.0326 ns | 0.0305 ns |  1.04 |    0.02 |         - |          NA |
