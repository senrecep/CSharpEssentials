```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  cold   : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **22.18 ms** | **0.334 ms** | **0.312 ms** |  **1.00** |    **0.02** |  **117.04 KB** |        **1.00** |
| CseGenerated  | 50  | 23.47 ms | 0.419 ms | 0.392 ms |  1.06 |    0.02 |  116.76 KB |        1.00 |
| CseReflection | 50  | 35.76 ms | 0.258 ms | 0.241 ms |  1.61 |    0.02 |  403.36 KB |        3.45 |
| Scrutor       | 50  | 31.48 ms | 0.497 ms | 0.465 ms |  1.42 |    0.03 |   181.3 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **49.24 ms** | **0.563 ms** | **0.527 ms** |  **1.00** |    **0.01** | **1187.49 KB** |        **1.00** |
| CseGenerated  | 500 | 51.63 ms | 0.606 ms | 0.567 ms |  1.05 |    0.02 | 1064.57 KB |        0.90 |
| CseReflection | 500 | 66.47 ms | 0.368 ms | 0.344 ms |  1.35 |    0.02 | 3871.52 KB |        3.26 |
| Scrutor       | 500 | 57.32 ms | 0.440 ms | 0.412 ms |  1.16 |    0.01 | 1723.06 KB |        1.45 |
