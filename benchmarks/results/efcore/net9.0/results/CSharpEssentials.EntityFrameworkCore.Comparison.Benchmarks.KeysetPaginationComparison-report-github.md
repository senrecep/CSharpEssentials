```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                              | Page  | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------- |------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_WhereOrderTake**                          | **After** | **127.62 μs** | **1.064 μs** | **0.995 μs** |  **1.00** |    **0.01** | **0.9766** |  **22.54 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | After | 131.77 μs | 1.068 μs | 0.999 μs |  1.03 |    0.01 | 1.4648 |  24.18 KB |        1.07 |
| CSharpEssentials_KeysetPaginateAsync_Func           | After | 146.95 μs | 0.264 μs | 0.221 μs |  1.15 |    0.01 | 1.4648 |  28.11 KB |        1.25 |
|                                                     |       |           |          |          |       |         |        |           |             |
| **HandWritten_WhereOrderTake**                          | **First** |  **81.99 μs** | **0.941 μs** | **0.880 μs** |  **1.00** |    **0.01** | **0.9766** |   **18.1 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | First |  79.02 μs | 0.853 μs | 0.798 μs |  0.96 |    0.01 | 0.9766 |  17.83 KB |        0.98 |
| CSharpEssentials_KeysetPaginateAsync_Func           | First |  99.56 μs | 0.751 μs | 0.702 μs |  1.21 |    0.02 | 1.2207 |  21.77 KB |        1.20 |
