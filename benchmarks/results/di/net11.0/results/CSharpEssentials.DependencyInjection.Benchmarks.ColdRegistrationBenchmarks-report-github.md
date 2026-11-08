```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host] : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **26.25 ms** | **0.385 ms** | **0.360 ms** |  **1.00** |    **0.02** |  **116.65 KB** |        **1.00** |
| CseGenerated  | 50  | 27.46 ms | 0.143 ms | 0.134 ms |  1.05 |    0.01 |  116.78 KB |        1.00 |
| CseReflection | 50  | 36.65 ms | 0.285 ms | 0.267 ms |  1.40 |    0.02 |  253.43 KB |        2.17 |
| Scrutor       | 50  | 36.10 ms | 0.328 ms | 0.306 ms |  1.38 |    0.02 |  180.51 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **60.69 ms** | **0.560 ms** | **0.524 ms** |  **1.00** |    **0.01** |  **1086.8 KB** |        **1.00** |
| CseGenerated  | 500 | 63.45 ms | 0.870 ms | 0.814 ms |  1.05 |    0.02 | 1170.74 KB |        1.08 |
| CseReflection | 500 | 71.22 ms | 0.503 ms | 0.470 ms |  1.17 |    0.01 | 2381.99 KB |        2.19 |
| Scrutor       | 500 | 69.71 ms | 0.770 ms | 0.720 ms |  1.15 |    0.01 | 1814.54 KB |        1.67 |
