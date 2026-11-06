```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                          | Succeed | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |-------- |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**                | **False**   | **2,399.324 ns** |  **9.2277 ns** |  **8.1801 ns** |  **1.00** |    **0.00** | **0.0305** |     **520 B** |        **1.00** |
| CSharpEssentials_ErrorException | False   | 7,742.162 ns | 34.0459 ns | 31.8466 ns |  3.23 |    0.02 | 0.3052 |    5569 B |       10.71 |
| CSharpFunctionalExtensions      | False   | 2,452.483 ns |  5.9080 ns |  5.2373 ns |  1.02 |    0.00 | 0.0267 |     464 B |        0.89 |
| ErrorOr                         | False   | 2,352.819 ns |  2.4837 ns |  2.3233 ns |  0.98 |    0.00 | 0.0305 |     552 B |        1.06 |
| FluentResults                   | False   | 2,642.237 ns |  9.3840 ns |  8.3187 ns |  1.10 |    0.00 | 0.0496 |     832 B |        1.60 |
|                                 |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials**                | **True**    |     **7.008 ns** |  **0.0508 ns** |  **0.0450 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_ErrorException | True    |     6.959 ns |  0.0154 ns |  0.0120 ns |  0.99 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions      | True    |     7.735 ns |  0.0024 ns |  0.0021 ns |  1.10 |    0.01 |      - |         - |          NA |
| ErrorOr                         | True    |     6.048 ns |  0.0017 ns |  0.0015 ns |  0.86 |    0.01 |      - |         - |          NA |
| FluentResults                   | True    |    29.261 ns |  0.1944 ns |  0.1818 ns |  4.18 |    0.04 | 0.0067 |     112 B |          NA |
