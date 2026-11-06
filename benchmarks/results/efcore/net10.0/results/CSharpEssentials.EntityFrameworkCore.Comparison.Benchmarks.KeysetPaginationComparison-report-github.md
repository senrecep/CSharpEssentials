```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                              | Page  | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------------------------------- |------ |----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_WhereOrderTake**                          | **After** | **119.54 μs** | **0.389 μs** | **0.345 μs** |  **1.00** |    **0.00** | **0.9766** |      **-** |  **23.15 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | After | 124.43 μs | 0.307 μs | 0.272 μs |  1.04 |    0.00 | 1.4648 |      - |  25.33 KB |        1.09 |
| CSharpEssentials_KeysetPaginateAsync_Func           | After | 137.49 μs | 0.302 μs | 0.252 μs |  1.15 |    0.00 | 1.4648 |      - |  28.95 KB |        1.25 |
| MR_KeysetPaginate_Prebuilt_TakePlusOne              | After | 145.81 μs | 0.540 μs | 0.478 μs |  1.22 |    0.01 | 1.4648 |      - |  30.38 KB |        1.31 |
| MR_KeysetPaginate_Inline_TakePlusOne                | After | 411.78 μs | 2.847 μs | 2.524 μs |  3.44 |    0.02 | 1.9531 | 0.9766 |  42.58 KB |        1.84 |
| MR_KeysetPaginate_Prebuilt_HasNextAsync             | After | 233.43 μs | 1.241 μs | 1.100 μs |  1.95 |    0.01 | 2.9297 |      - |  48.59 KB |        2.10 |
|                                                     |       |           |          |          |       |         |        |        |           |             |
| **HandWritten_WhereOrderTake**                          | **First** |  **75.46 μs** | **0.234 μs** | **0.195 μs** |  **1.00** |    **0.00** | **0.9766** |      **-** |  **18.26 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | First |  72.12 μs | 0.128 μs | 0.107 μs |  0.96 |    0.00 | 0.9766 |      - |  17.88 KB |        0.98 |
| CSharpEssentials_KeysetPaginateAsync_Func           | First |  87.31 μs | 0.153 μs | 0.135 μs |  1.16 |    0.00 | 1.2207 |      - |  21.96 KB |        1.20 |
| MR_KeysetPaginate_Prebuilt_TakePlusOne              | First |  72.24 μs | 0.222 μs | 0.197 μs |  0.96 |    0.00 | 0.9766 |      - |  17.97 KB |        0.98 |
| MR_KeysetPaginate_Inline_TakePlusOne                | First |  78.66 μs | 0.208 μs | 0.174 μs |  1.04 |    0.00 | 1.2207 |      - |  21.06 KB |        1.15 |
| MR_KeysetPaginate_Prebuilt_HasNextAsync             | First | 173.53 μs | 2.379 μs | 2.109 μs |  2.30 |    0.03 | 1.9531 |      - |  36.13 KB |        1.98 |
