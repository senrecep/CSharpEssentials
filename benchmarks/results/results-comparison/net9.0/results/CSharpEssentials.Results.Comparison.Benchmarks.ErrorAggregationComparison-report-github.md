```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **423.18 ns** | **2.573 ns** | **2.407 ns** |  **1.00** |    **0.01** | **0.0415** |     **696 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 451.52 ns | 0.980 ns | 0.917 ns |  1.07 |    0.01 | 0.0467 |     784 B |        1.13 |
| ErrorOr                    | False   |  46.07 ns | 0.939 ns | 1.187 ns |  0.11 |    0.00 | 0.0110 |     184 B |        0.26 |
| FluentResults              | False   | 274.78 ns | 2.569 ns | 2.403 ns |  0.65 |    0.01 | 0.0424 |     712 B |        1.02 |
|                            |         |           |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **32.88 ns** | **0.224 ns** | **0.209 ns** |  **1.00** |    **0.01** | **0.0067** |     **112 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  53.81 ns | 0.188 ns | 0.176 ns |  1.64 |    0.01 | 0.0095 |     160 B |        1.43 |
| ErrorOr                    | True    |  11.53 ns | 0.060 ns | 0.056 ns |  0.35 |    0.00 | 0.0019 |      32 B |        0.29 |
| FluentResults              | True    | 141.57 ns | 0.881 ns | 0.781 ns |  4.31 |    0.04 | 0.0219 |     368 B |        3.29 |
