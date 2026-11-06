```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.4930 ns** | **0.0036 ns** | **0.0032 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.0072 ns | 0.0179 ns | 0.0167 ns |  2.04 |    0.04 |         - |          NA |
| ErrorOr                    | False   | 2.0245 ns | 0.0017 ns | 0.0015 ns |  4.11 |    0.03 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.5426 ns** | **0.0062 ns** | **0.0058 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.7568 ns | 0.0144 ns | 0.0135 ns |  1.14 |    0.01 |         - |          NA |
| ErrorOr                    | True    | 1.8978 ns | 0.0184 ns | 0.0172 ns |  1.23 |    0.01 |         - |          NA |
