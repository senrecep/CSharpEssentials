```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.4904 ns** | **0.0014 ns** | **0.0013 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.1976 ns | 0.0086 ns | 0.0076 ns |  2.44 |    0.02 |         - |          NA |
| ErrorOr                    | False   | 0.7186 ns | 0.0043 ns | 0.0040 ns |  1.47 |    0.01 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.5846 ns** | **0.0118 ns** | **0.0110 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.9116 ns | 0.0554 ns | 0.0518 ns |  1.21 |    0.03 |         - |          NA |
| ErrorOr                    | True    | 1.5974 ns | 0.0221 ns | 0.0196 ns |  1.01 |    0.01 |         - |          NA |
