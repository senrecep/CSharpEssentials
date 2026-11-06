```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                         | PageNumber | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **Ef_CountSkipTake**               | **1**          | **104.2 μs** | **0.44 μs** | **0.39 μs** |  **1.00** |    **0.01** | **0.9766** |  **19.37 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 1          | 110.2 μs | 2.09 μs | 1.96 μs |  1.06 |    0.02 | 0.9766 |  19.56 KB |        1.01 |
|                                |            |          |         |         |       |         |        |           |             |
| **Ef_CountSkipTake**               | **250**        | **147.9 μs** | **0.27 μs** | **0.22 μs** |  **1.00** |    **0.00** | **0.9766** |  **19.37 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 250        | 149.7 μs | 0.27 μs | 0.24 μs |  1.01 |    0.00 | 0.9766 |  19.41 KB |        1.00 |
