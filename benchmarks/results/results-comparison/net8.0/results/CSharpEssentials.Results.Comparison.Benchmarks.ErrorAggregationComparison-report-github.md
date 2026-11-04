```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **214.051 ns** | **2.4455 ns** | **2.2875 ns** |  **1.00** |    **0.01** | **0.0415** |     **696 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 434.374 ns | 1.7276 ns | 1.6160 ns |  2.03 |    0.02 | 0.0477 |     800 B |        1.15 |
| ErrorOr                    | False   |  46.258 ns | 0.1883 ns | 0.1572 ns |  0.22 |    0.00 | 0.0110 |     184 B |        0.26 |
| FluentResults              | False   | 254.005 ns | 1.0604 ns | 0.9919 ns |  1.19 |    0.01 | 0.0424 |     712 B |        1.02 |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **30.781 ns** | **0.2188 ns** | **0.2047 ns** |  **1.00** |    **0.01** | **0.0067** |     **112 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  52.238 ns | 0.2710 ns | 0.2263 ns |  1.70 |    0.01 | 0.0095 |     160 B |        1.43 |
| ErrorOr                    | True    |   8.056 ns | 0.1364 ns | 0.1276 ns |  0.26 |    0.00 | 0.0019 |      32 B |        0.29 |
| FluentResults              | True    | 148.454 ns | 0.9176 ns | 0.8583 ns |  4.82 |    0.04 | 0.0219 |     368 B |        3.29 |
