```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **116.756 ns** | **1.2807 ns** | **1.1979 ns** |  **1.00** |    **0.01** | **0.0200** |     **336 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | 458.489 ns | 2.5280 ns | 2.3647 ns |  3.93 |    0.04 | 0.0477 |     800 B |        2.38 |
| ErrorOr                    | False   |  47.569 ns | 0.3044 ns | 0.2848 ns |  0.41 |    0.00 | 0.0110 |     184 B |        0.55 |
| FluentResults              | False   | 292.322 ns | 1.9323 ns | 1.7129 ns |  2.50 |    0.03 | 0.0424 |     712 B |        2.12 |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **17.040 ns** | **0.1887 ns** | **0.1765 ns** |  **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions | True    |  54.476 ns | 0.2188 ns | 0.2047 ns |  3.20 |    0.03 | 0.0095 |     160 B |        3.33 |
| ErrorOr                    | True    |   8.318 ns | 0.0670 ns | 0.0594 ns |  0.49 |    0.01 | 0.0019 |      32 B |        0.67 |
| FluentResults              | True    | 151.108 ns | 1.1178 ns | 1.0456 ns |  8.87 |    0.11 | 0.0219 |     368 B |        7.67 |
