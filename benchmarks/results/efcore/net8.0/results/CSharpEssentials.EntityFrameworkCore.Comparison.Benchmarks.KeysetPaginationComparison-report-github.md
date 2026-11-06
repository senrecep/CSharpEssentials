```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                              | Page  | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------- |------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_WhereOrderTake**                          | **After** | **138.85 μs** | **1.172 μs** | **0.979 μs** |  **1.00** |    **0.01** | **0.9766** |  **19.86 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | After | 140.99 μs | 0.418 μs | 0.370 μs |  1.02 |    0.01 | 0.9766 |  21.34 KB |        1.07 |
| CSharpEssentials_KeysetPaginateAsync_Func           | After | 163.95 μs | 1.894 μs | 1.771 μs |  1.18 |    0.01 | 1.4648 |  25.74 KB |        1.30 |
|                                                     |       |           |          |          |       |         |        |           |             |
| **HandWritten_WhereOrderTake**                          | **First** |  **87.87 μs** | **0.162 μs** | **0.135 μs** |  **1.00** |    **0.00** | **0.7324** |  **15.79 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | First |  84.37 μs | 0.310 μs | 0.290 μs |  0.96 |    0.00 | 0.7324 |  15.52 KB |        0.98 |
| CSharpEssentials_KeysetPaginateAsync_Func           | First | 110.80 μs | 1.575 μs | 1.473 μs |  1.26 |    0.02 | 0.9766 |  19.48 KB |        1.23 |
