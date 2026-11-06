```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                          | Succeed | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |-------- |--------------:|------------:|------------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**                | **False**   |  **8,672.053 ns** |  **23.9442 ns** |  **19.9945 ns** |  **1.00** |    **0.00** | **0.0305** |     **544 B** |        **1.00** |
| CSharpEssentials_ErrorException | False   | 15,823.105 ns | 121.2371 ns | 101.2385 ns |  1.82 |    0.01 | 0.3052 |    5665 B |       10.41 |
| CSharpFunctionalExtensions      | False   |  9,834.317 ns |  21.2731 ns |  19.8989 ns |  1.13 |    0.00 | 0.0305 |     704 B |        1.29 |
| ErrorOr                         | False   |  8,807.307 ns |  64.2107 ns |  60.0627 ns |  1.02 |    0.01 | 0.0305 |     576 B |        1.06 |
| FluentResults                   | False   |  9,210.607 ns |  34.7632 ns |  30.8166 ns |  1.06 |    0.00 | 0.0458 |     872 B |        1.60 |
|                                 |         |               |             |             |       |         |        |           |             |
| **CSharpEssentials**                | **True**    |      **7.838 ns** |   **0.0317 ns** |   **0.0297 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_ErrorException | True    |      7.537 ns |   0.0424 ns |   0.0396 ns |  0.96 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions      | True    |      9.716 ns |   0.0209 ns |   0.0196 ns |  1.24 |    0.01 |      - |         - |          NA |
| ErrorOr                         | True    |      6.586 ns |   0.0010 ns |   0.0008 ns |  0.84 |    0.00 |      - |         - |          NA |
| FluentResults                   | True    |     50.520 ns |   0.1514 ns |   0.1342 ns |  6.45 |    0.03 | 0.0072 |     120 B |          NA |
