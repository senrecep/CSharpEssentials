```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host] : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  cold   : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **22.76 ms** | **0.510 ms** | **0.477 ms** |  **1.00** |    **0.03** |  **117.04 KB** |        **1.00** |
| CseGenerated  | 50  | 23.82 ms | 0.327 ms | 0.306 ms |  1.05 |    0.02 |  116.76 KB |        1.00 |
| CseReflection | 50  | 33.30 ms | 0.320 ms | 0.300 ms |  1.46 |    0.03 |  317.89 KB |        2.72 |
| Scrutor       | 50  | 32.12 ms | 0.561 ms | 0.525 ms |  1.41 |    0.04 |   181.3 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **50.62 ms** | **0.859 ms** | **0.803 ms** |  **1.00** |    **0.02** | **1187.49 KB** |        **1.00** |
| CseGenerated  | 500 | 52.97 ms | 0.761 ms | 0.712 ms |  1.05 |    0.02 | 1112.57 KB |        0.94 |
| CseReflection | 500 | 62.68 ms | 0.373 ms | 0.349 ms |  1.24 |    0.02 | 3016.13 KB |        2.54 |
| Scrutor       | 500 | 58.24 ms | 0.607 ms | 0.568 ms |  1.15 |    0.02 | 1819.11 KB |        1.53 |
