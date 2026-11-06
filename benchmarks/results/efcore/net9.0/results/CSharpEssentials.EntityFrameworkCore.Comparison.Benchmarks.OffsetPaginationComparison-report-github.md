```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                         | PageNumber | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **Ef_CountSkipTake**               | **1**          | **101.1 μs** | **0.26 μs** | **0.21 μs** |  **1.00** | **0.9766** |  **23.43 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 1          | 100.4 μs | 0.21 μs | 0.19 μs |  0.99 | 0.9766 |  23.47 KB |        1.00 |
| XPagedList_ToPagedListAsync    | 1          | 100.3 μs | 0.29 μs | 0.26 μs |  0.99 | 0.9766 |   23.7 KB |        1.01 |
|                                |            |          |         |         |       |        |           |             |
| **Ef_CountSkipTake**               | **250**        | **143.5 μs** | **0.33 μs** | **0.30 μs** |  **1.00** | **0.9766** |  **23.43 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 250        | 142.2 μs | 0.25 μs | 0.22 μs |  0.99 | 0.9766 |  23.47 KB |        1.00 |
| XPagedList_ToPagedListAsync    | 250        | 142.8 μs | 0.23 μs | 0.21 μs |  1.00 | 0.9766 |   23.7 KB |        1.01 |
