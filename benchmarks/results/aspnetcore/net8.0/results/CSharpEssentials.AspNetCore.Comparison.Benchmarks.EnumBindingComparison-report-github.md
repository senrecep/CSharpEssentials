```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Input      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_NoMiddleware**                       | **Bind**        | **Canonical**  | **11.127 μs** | **0.2207 μs** | **0.1957 μs** |  **1.00** |    **0.02** | **0.4883** |   **9.55 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Canonical  | 12.335 μs | 0.2467 μs | 0.5415 μs |  1.11 |    0.05 | 0.6104 |  10.65 KB |        1.12 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Invalid**    |  **8.704 μs** | **0.1113 μs** | **0.0929 μs** |  **1.00** |    **0.01** | **0.4883** |   **8.31 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Invalid    | 18.802 μs | 0.3736 μs | 0.7374 μs |  2.16 |    0.09 | 0.7324 |  13.26 KB |        1.60 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Normalized** |  **8.591 μs** | **0.1375 μs** | **0.1219 μs** |  **1.00** |    **0.02** | **0.4883** |   **8.45 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Normalized | 12.769 μs | 0.2497 μs | 0.2973 μs |  1.49 |    0.04 | 0.6104 |  11.36 KB |        1.34 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Canonical**  |  **9.826 μs** | **0.1845 μs** | **0.1895 μs** |  **1.00** |    **0.03** | **0.4883** |   **8.92 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Canonical  | 10.205 μs | 0.1978 μs | 0.2773 μs |  1.04 |    0.03 | 0.4883 |   8.98 KB |        1.01 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Invalid**    | **10.155 μs** | **0.2018 μs** | **0.3372 μs** |  **1.00** |    **0.05** | **0.4883** |   **8.92 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Invalid    | 10.124 μs | 0.1910 μs | 0.1694 μs |  1.00 |    0.04 | 0.4883 |   8.98 KB |        1.01 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Normalized** |  **9.978 μs** | **0.1940 μs** | **0.2782 μs** |  **1.00** |    **0.04** | **0.4883** |   **8.92 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Normalized |  9.994 μs | 0.1852 μs | 0.2714 μs |  1.00 |    0.04 | 0.4883 |   8.98 KB |        1.01 |
