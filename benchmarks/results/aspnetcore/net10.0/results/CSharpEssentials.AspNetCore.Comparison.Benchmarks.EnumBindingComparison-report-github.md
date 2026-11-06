```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Input      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_NoMiddleware**                       | **Bind**        | **Canonical**  |  **9.428 μs** | **0.1760 μs** | **0.1883 μs** |  **1.00** |    **0.03** | **0.4883** |   **9.09 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Canonical  | 10.604 μs | 0.2112 μs | 0.2819 μs |  1.13 |    0.04 | 0.6104 |  10.14 KB |        1.12 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Invalid**    |  **7.304 μs** | **0.1314 μs** | **0.1164 μs** |  **1.00** |    **0.02** | **0.4883** |   **8.16 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Invalid    | 14.532 μs | 0.2644 μs | 0.2344 μs |  1.99 |    0.04 | 0.7324 |   12.8 KB |        1.57 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Normalized** |  **7.313 μs** | **0.1010 μs** | **0.0895 μs** |  **1.00** |    **0.02** | **0.4883** |   **8.26 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Normalized | 11.048 μs | 0.1658 μs | 0.1384 μs |  1.51 |    0.03 | 0.6104 |  10.79 KB |        1.31 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Canonical**  |  **8.485 μs** | **0.1454 μs** | **0.1289 μs** |  **1.00** |    **0.02** | **0.4883** |   **8.44 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Canonical  |  8.636 μs | 0.1664 μs | 0.1981 μs |  1.02 |    0.03 | 0.4883 |   8.44 KB |        1.00 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Invalid**    |  **8.411 μs** | **0.1625 μs** | **0.1520 μs** |  **1.00** |    **0.02** | **0.4883** |   **8.44 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Invalid    |  8.535 μs | 0.1193 μs | 0.1116 μs |  1.02 |    0.02 | 0.4883 |   8.44 KB |        1.00 |
|                                        |             |            |           |           |           |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Normalized** |  **8.362 μs** | **0.1048 μs** | **0.0818 μs** |  **1.00** |    **0.01** | **0.4883** |   **8.44 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Normalized |  8.608 μs | 0.1229 μs | 0.1089 μs |  1.03 |    0.02 | 0.4883 |   8.44 KB |        1.00 |
