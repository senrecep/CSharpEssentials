```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.5006 ns** | **0.0039 ns** | **0.0035 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.1046 ns | 0.0083 ns | 0.0074 ns |  2.21 |    0.02 |         - |          NA |
| ErrorOr                    | False   | 0.8163 ns | 0.0056 ns | 0.0050 ns |  1.63 |    0.01 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.6085 ns** | **0.0280 ns** | **0.0262 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.7265 ns | 0.0063 ns | 0.0059 ns |  1.07 |    0.02 |         - |          NA |
| ErrorOr                    | True    | 1.5741 ns | 0.0030 ns | 0.0026 ns |  0.98 |    0.02 |         - |          NA |
