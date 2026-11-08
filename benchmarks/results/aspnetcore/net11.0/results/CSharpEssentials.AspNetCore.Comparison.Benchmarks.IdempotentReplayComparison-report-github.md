```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                                | Categories | ResponseSize | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|-------------------------------------- |----------- |------------- |------------:|----------:|----------:|------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| **Plain_NoIdempotency_NewKey**            | **NewKey**     | **1024**         |    **14.65 μs** |  **0.285 μs** |  **0.370 μs** |  **1.00** |    **0.03** |   **0.7324** |        **-** |        **-** |   **13.34 KB** |        **1.00** |
| CSharpEssentials_NewKey               | NewKey     | 1024         |    36.21 μs |  0.718 μs |  1.482 μs |  2.47 |    0.12 |   1.2207 |   0.2441 |        - |   22.64 KB |        1.70 |
| IdempotentAPI_NewKey                  | NewKey     | 1024         |   106.81 μs |  2.046 μs |  1.914 μs |  7.29 |    0.22 |   3.4180 |   0.9766 |        - |   58.65 KB |        4.40 |
|                                       |            |              |             |           |           |       |         |          |          |          |            |             |
| **Plain_NoIdempotency_NewKey**            | **NewKey**     | **262144**       |    **84.26 μs** |  **0.978 μs** |  **0.914 μs** |  **1.00** |    **0.01** |  **22.4609** |  **21.9727** |  **21.9727** |  **270.45 KB** |        **1.00** |
| CSharpEssentials_NewKey               | NewKey     | 262144       |   190.68 μs |  3.381 μs |  4.628 μs |  2.26 |    0.06 |   1.9531 |   0.9766 |   0.9766 |  798.02 KB |        2.95 |
| IdempotentAPI_NewKey                  | NewKey     | 262144       | 5,210.03 μs | 11.243 μs |  9.389 μs | 61.84 |    0.66 |   7.8125 |        - |        - | 2274.51 KB |        8.41 |
|                                       |            |              |             |           |           |       |         |          |          |          |            |             |
| **Plain_NoIdempotency**                   | **Replay**     | **1024**         |    **13.22 μs** |  **0.262 μs** |  **0.511 μs** |  **1.00** |    **0.05** |   **0.7324** |        **-** |        **-** |   **13.22 KB** |        **1.00** |
| CSharpEssentials_Replay               | Replay     | 1024         |    15.01 μs |  0.286 μs |  0.340 μs |  1.14 |    0.05 |   0.7324 |        - |        - |   14.72 KB |        1.11 |
| IdempotentAPI_Replay                  | Replay     | 1024         |    80.02 μs |  1.354 μs |  1.267 μs |  6.06 |    0.24 |   2.9297 |        - |        - |   48.98 KB |        3.70 |
| CSharpEssentials_Replay_InMemoryStore | Replay     | 1024         |    14.74 μs |  0.293 μs |  0.456 μs |  1.12 |    0.05 |   0.7324 |        - |        - |      14 KB |        1.06 |
|                                       |            |              |             |           |           |       |         |          |          |          |            |             |
| **Plain_NoIdempotency**                   | **Replay**     | **262144**       |    **96.44 μs** |  **1.850 μs** |  **1.817 μs** |  **1.00** |    **0.03** |  **22.9492** |  **22.4609** |  **22.4609** |  **270.47 KB** |        **1.00** |
| CSharpEssentials_Replay               | Replay     | 262144       |    94.11 μs |  1.324 μs |  1.174 μs |  0.98 |    0.02 |  30.7617 |  29.7852 |  29.7852 |  278.24 KB |        1.03 |
| IdempotentAPI_Replay                  | Replay     | 262144       | 1,443.11 μs | 21.036 μs | 19.677 μs | 14.97 |    0.33 | 396.4844 | 386.7188 | 386.7188 | 4518.51 KB |       16.71 |
| CSharpEssentials_Replay_InMemoryStore | Replay     | 262144       |    94.58 μs |  1.858 μs |  1.908 μs |  0.98 |    0.03 |  30.2734 |  29.2969 |  29.2969 |   277.6 KB |        1.03 |
