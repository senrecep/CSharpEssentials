```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **0.5289 ns** | **0.0110 ns** | **0.0103 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   | 1.1427 ns | 0.0164 ns | 0.0154 ns |  2.16 |    0.05 |         - |          NA |
| ErrorOr                    | False   | 1.7525 ns | 0.0007 ns | 0.0006 ns |  3.31 |    0.06 |         - |          NA |
|                            |         |           |           |           |       |         |           |             |
| **CSharpEssentials**           | **True**    | **1.6047 ns** | **0.0102 ns** | **0.0096 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    | 1.7455 ns | 0.0187 ns | 0.0166 ns |  1.09 |    0.01 |         - |          NA |
| ErrorOr                    | True    | 1.7868 ns | 0.0188 ns | 0.0176 ns |  1.11 |    0.01 |         - |          NA |
