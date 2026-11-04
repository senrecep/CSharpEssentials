```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.6557 ns** | **0.0022 ns** | **0.0021 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.1940 ns | 0.0030 ns | 0.0027 ns |  1.82 |    0.01 |         - |          NA |
| ErrorOr                    | False   | 0.8153 ns | 0.0019 ns | 0.0017 ns |  1.24 |    0.00 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.6179 ns** | **0.0309 ns** | **0.0289 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.9231 ns | 0.0129 ns | 0.0115 ns |  1.19 |    0.02 |         - |          NA |
| ErrorOr                    | True    | 1.5970 ns | 0.0184 ns | 0.0153 ns |  0.99 |    0.02 |         - |          NA |
