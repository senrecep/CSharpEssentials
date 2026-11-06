```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                 | Categories  | Input      | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------------- |------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_NoMiddleware**                       | **Bind**        | **Canonical**  | **13.22 μs** | **0.246 μs** | **0.360 μs** |  **1.00** |    **0.04** | **0.6104** |  **10.51 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Canonical  | 13.25 μs | 0.221 μs | 0.227 μs |  1.00 |    0.03 | 0.6104 |  11.56 KB |        1.10 |
|                                        |             |            |          |          |          |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Invalid**    | **11.74 μs** | **0.209 μs** | **0.241 μs** |  **1.00** |    **0.03** | **0.4883** |   **9.69 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Invalid    | 18.24 μs | 0.343 μs | 0.685 μs |  1.55 |    0.07 | 0.7324 |  14.22 KB |        1.47 |
|                                        |             |            |          |          |          |       |         |        |           |             |
| **Bcl_NoMiddleware**                       | **Bind**        | **Normalized** | **11.85 μs** | **0.233 μs** | **0.402 μs** |  **1.00** |    **0.05** | **0.4883** |   **9.73 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding        | Bind        | Normalized | 13.51 μs | 0.261 μs | 0.366 μs |  1.14 |    0.05 | 0.7324 |  12.21 KB |        1.25 |
|                                        |             |            |          |          |          |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Canonical**  | **12.73 μs** | **0.245 μs** | **0.292 μs** |  **1.00** |    **0.03** | **0.4883** |   **9.87 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Canonical  | 12.53 μs | 0.244 μs | 0.325 μs |  0.98 |    0.03 | 0.4883 |   9.86 KB |        1.00 |
|                                        |             |            |          |          |          |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Invalid**    | **12.71 μs** | **0.251 μs** | **0.343 μs** |  **1.00** |    **0.04** | **0.4883** |   **9.86 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Invalid    | 12.86 μs | 0.257 μs | 0.264 μs |  1.01 |    0.03 | 0.4883 |   9.86 KB |        1.00 |
|                                        |             |            |          |          |          |       |         |        |           |             |
| **Bcl_NoMiddleware_NoEnum**                | **PassThrough** | **Normalized** | **12.78 μs** | **0.250 μs** | **0.297 μs** |  **1.00** |    **0.03** | **0.4883** |   **9.87 KB** |        **1.00** |
| CSharpEssentials_UseEnumBinding_NoEnum | PassThrough | Normalized | 12.91 μs | 0.256 μs | 0.285 μs |  1.01 |    0.03 | 0.4883 |   9.86 KB |        1.00 |
