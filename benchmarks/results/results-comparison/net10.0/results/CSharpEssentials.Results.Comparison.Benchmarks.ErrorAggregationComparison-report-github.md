```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **99.09 ns** | **0.451 ns** | **0.400 ns** |  **1.00** |    **0.01** | **0.0200** |     **336 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 368.58 ns | 1.253 ns | 1.172 ns |  3.72 |    0.02 | 0.0467 |     784 B |        2.33 |
| ErrorOr                    | False   |  42.79 ns | 0.166 ns | 0.147 ns |  0.43 |    0.00 | 0.0110 |     184 B |        0.55 |
| FluentResults              | False   | 212.03 ns | 0.956 ns | 0.746 ns |  2.14 |    0.01 | 0.0424 |     712 B |        2.12 |
|                            |         |           |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **14.51 ns** | **0.135 ns** | **0.126 ns** |  **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  31.65 ns | 0.233 ns | 0.218 ns |  2.18 |    0.02 | 0.0095 |     160 B |        3.33 |
| ErrorOr                    | True    |  11.63 ns | 0.038 ns | 0.032 ns |  0.80 |    0.01 | 0.0019 |      32 B |        0.67 |
| FluentResults              | True    | 106.85 ns | 0.689 ns | 0.644 ns |  7.37 |    0.08 | 0.0219 |     368 B |        7.67 |
