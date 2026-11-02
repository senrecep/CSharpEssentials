```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.4896 ns** | **0.0027 ns** | **0.0024 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.1964 ns | 0.0063 ns | 0.0059 ns |  2.44 |    0.02 |         - |          NA |
| ErrorOr                    | False   | 0.7160 ns | 0.0030 ns | 0.0028 ns |  1.46 |    0.01 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.5295 ns** | **0.0074 ns** | **0.0069 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.9379 ns | 0.0476 ns | 0.0422 ns |  1.27 |    0.03 |         - |          NA |
| ErrorOr                    | True    | 1.5920 ns | 0.0086 ns | 0.0080 ns |  1.04 |    0.01 |         - |          NA |
