```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                          | Succeed | Mean          | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |-------- |--------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**                | **False**   |  **3,879.014 ns** |  **5.1938 ns** |  **4.0550 ns** |  **1.00** |    **0.00** | **0.0305** |     **520 B** |        **1.00** |
| CSharpEssentials_ErrorException | False   | 11,136.572 ns | 65.5577 ns | 58.1152 ns |  2.87 |    0.01 | 0.3052 |    5569 B |       10.71 |
| CSharpFunctionalExtensions      | False   |  3,863.613 ns | 14.1023 ns | 13.1913 ns |  1.00 |    0.00 | 0.0229 |     464 B |        0.89 |
| ErrorOr                         | False   |  3,914.338 ns | 28.2488 ns | 25.0418 ns |  1.01 |    0.01 | 0.0305 |     552 B |        1.06 |
| FluentResults                   | False   |  4,125.503 ns |  7.0408 ns |  6.5860 ns |  1.06 |    0.00 | 0.0458 |     832 B |        1.60 |
|                                 |         |               |            |            |       |         |        |           |             |
| **CSharpEssentials**                | **True**    |      **6.952 ns** |  **0.0417 ns** |  **0.0390 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_ErrorException | True    |      7.172 ns |  0.0014 ns |  0.0011 ns |  1.03 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions      | True    |      9.779 ns |  0.0189 ns |  0.0158 ns |  1.41 |    0.01 |      - |         - |          NA |
| ErrorOr                         | True    |      6.045 ns |  0.0019 ns |  0.0017 ns |  0.87 |    0.00 |      - |         - |          NA |
| FluentResults                   | True    |     40.108 ns |  0.1910 ns |  0.1787 ns |  5.77 |    0.04 | 0.0067 |     112 B |          NA |
