```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                              | Page  | Mean      | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------------------------------- |------ |----------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
| **HandWritten_WhereOrderTake**                          | **After** | **113.27 μs** | **0.259 μs** | **0.230 μs** |  **1.00** | **0.9766** |      **-** |   **23.1 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | After | 112.84 μs | 0.223 μs | 0.198 μs |  1.00 | 1.4648 |      - |  25.13 KB |        1.09 |
| CSharpEssentials_KeysetPaginateAsync_Func           | After | 128.46 μs | 0.266 μs | 0.236 μs |  1.13 | 1.4648 |      - |  29.13 KB |        1.26 |
| MR_KeysetPaginate_Prebuilt_TakePlusOne              | After | 134.76 μs | 0.272 μs | 0.227 μs |  1.19 | 1.4648 |      - |  30.54 KB |        1.32 |
| MR_KeysetPaginate_Inline_TakePlusOne                | After | 413.32 μs | 0.634 μs | 0.529 μs |  3.65 | 1.9531 | 0.9766 |  42.52 KB |        1.84 |
| MR_KeysetPaginate_Prebuilt_HasNextAsync             | After | 217.47 μs | 0.853 μs | 0.798 μs |  1.92 | 2.9297 |      - |  47.98 KB |        2.08 |
|                                                     |       |           |          |          |       |        |        |           |             |
| **HandWritten_WhereOrderTake**                          | **First** |  **70.23 μs** | **0.135 μs** | **0.120 μs** |  **1.00** | **0.9766** |      **-** |  **18.08 KB** |        **1.00** |
| CSharpEssentials_KeysetPaginateAsync_CachedOrdering | First |  68.13 μs | 0.200 μs | 0.178 μs |  0.97 | 0.9766 |      - |  17.81 KB |        0.99 |
| CSharpEssentials_KeysetPaginateAsync_Func           | First |  82.95 μs | 0.186 μs | 0.165 μs |  1.18 | 0.9766 |      - |  21.74 KB |        1.20 |
| MR_KeysetPaginate_Prebuilt_TakePlusOne              | First |  69.67 μs | 0.164 μs | 0.145 μs |  0.99 | 0.9766 |      - |  17.81 KB |        0.99 |
| MR_KeysetPaginate_Inline_TakePlusOne                | First |  71.51 μs | 0.144 μs | 0.128 μs |  1.02 | 1.2207 |      - |  20.89 KB |        1.16 |
| MR_KeysetPaginate_Prebuilt_HasNextAsync             | First | 156.74 μs | 0.248 μs | 0.220 μs |  2.23 | 1.9531 |      - |   35.7 KB |        1.97 |
