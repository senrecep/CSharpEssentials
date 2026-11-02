```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **237.91 ns** | **1.341 ns** | **1.189 ns** |  **1.00** |    **0.01** | **0.0415** |     **696 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 372.08 ns | 2.657 ns | 2.485 ns |  1.56 |    0.01 | 0.0467 |     784 B |        1.13 |
| ErrorOr                    | False   |  41.86 ns | 0.168 ns | 0.158 ns |  0.18 |    0.00 | 0.0110 |     184 B |        0.26 |
| FluentResults              | False   | 205.11 ns | 1.002 ns | 0.888 ns |  0.86 |    0.01 | 0.0424 |     712 B |        1.02 |
|                            |         |           |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **16.01 ns** | **0.089 ns** | **0.079 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  30.80 ns | 0.274 ns | 0.243 ns |  1.92 |    0.02 | 0.0095 |     160 B |        2.00 |
| ErrorOr                    | True    |  11.27 ns | 0.049 ns | 0.044 ns |  0.70 |    0.00 | 0.0019 |      32 B |        0.40 |
| FluentResults              | True    | 100.54 ns | 0.488 ns | 0.433 ns |  6.28 |    0.04 | 0.0219 |     368 B |        4.60 |
