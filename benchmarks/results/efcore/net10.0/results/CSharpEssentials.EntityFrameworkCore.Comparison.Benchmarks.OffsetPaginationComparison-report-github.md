```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                         | PageNumber | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **Ef_CountSkipTake**               | **1**          |  **90.74 μs** | **0.941 μs** | **0.834 μs** |  **1.00** | **0.9766** |  **23.59 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 1          |  91.74 μs | 0.437 μs | 0.365 μs |  1.01 | 0.9766 |  23.63 KB |        1.00 |
| XPagedList_ToPagedListAsync    | 1          |  90.38 μs | 0.253 μs | 0.211 μs |  1.00 | 0.9766 |  23.86 KB |        1.01 |
|                                |            |           |          |          |       |        |           |             |
| **Ef_CountSkipTake**               | **250**        | **132.06 μs** | **0.220 μs** | **0.184 μs** |  **1.00** | **0.9766** |  **23.59 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 250        | 136.71 μs | 0.920 μs | 0.861 μs |  1.04 | 1.4648 |  24.24 KB |        1.03 |
| XPagedList_ToPagedListAsync    | 250        | 136.60 μs | 0.281 μs | 0.249 μs |  1.03 | 1.4648 |     24 KB |        1.02 |
