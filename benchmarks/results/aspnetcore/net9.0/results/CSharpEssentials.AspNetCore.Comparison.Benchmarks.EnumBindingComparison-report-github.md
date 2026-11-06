```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Input      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_NoMiddleware**                       | **Bind**        | **Canonical**  | **10.262 μs** | **0.1309 μs** | **0.1160 μs** |  **1.00** |    **0.02** | **0.5493** |   **9.44 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Canonical  | 11.604 μs | 0.2254 μs | 0.1998 μs |  1.13 |    0.02 | 0.6104 |  10.55 KB |        1.12 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Invalid**    |  **8.263 μs** | **0.1405 μs** | **0.1315 μs** |  **1.00** |    **0.02** | **0.4883** |   **8.29 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Invalid    | 17.283 μs | 0.3257 μs | 0.3750 μs |  2.09 |    0.05 | 0.7324 |  13.08 KB |        1.58 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Normalized** |  **8.058 μs** | **0.0748 μs** | **0.0625 μs** |  **1.00** |    **0.01** | **0.4883** |   **8.38 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Normalized | 12.380 μs | 0.2229 μs | 0.3197 μs |  1.54 |    0.04 | 0.6104 |  11.21 KB |        1.34 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Canonical**  |  **9.420 μs** | **0.1665 μs** | **0.1782 μs** |  **1.00** |    **0.03** | **0.4883** |   **8.81 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Canonical  |  9.442 μs | 0.1700 μs | 0.1590 μs |  1.00 |    0.02 | 0.4883 |   8.87 KB |        1.01 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Invalid**    |  **9.851 μs** | **0.1940 μs** | **0.3498 μs** |  **1.00** |    **0.05** | **0.4883** |   **8.81 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Invalid    |  9.623 μs | 0.1903 μs | 0.2848 μs |  0.98 |    0.04 | 0.4883 |   8.87 KB |        1.01 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Normalized** |  **9.420 μs** | **0.1848 μs** | **0.1729 μs** |  **1.00** |    **0.03** | **0.4883** |   **8.81 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Normalized |  9.542 μs | 0.1877 μs | 0.1664 μs |  1.01 |    0.02 | 0.4883 |   8.87 KB |        1.01 |
