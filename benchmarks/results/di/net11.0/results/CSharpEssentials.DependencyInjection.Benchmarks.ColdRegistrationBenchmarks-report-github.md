```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **26.24 ms** | **0.694 ms** | **0.649 ms** |  **1.00** |    **0.03** |   **116.6 KB** |        **1.00** |
| CseGenerated  | 50  | 27.55 ms | 0.726 ms | 0.679 ms |  1.05 |    0.03 |  116.45 KB |        1.00 |
| CseReflection | 50  | 39.84 ms | 0.352 ms | 0.329 ms |  1.52 |    0.04 |  332.37 KB |        2.85 |
| Scrutor       | 50  | 36.05 ms | 0.382 ms | 0.358 ms |  1.37 |    0.03 |  180.51 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **60.65 ms** | **0.267 ms** | **0.250 ms** |  **1.00** |    **0.01** | **1087.08 KB** |        **1.00** |
| CseGenerated  | 500 | 63.34 ms | 0.568 ms | 0.531 ms |  1.04 |    0.01 | 1074.65 KB |        0.99 |
| CseReflection | 500 | 76.43 ms | 0.732 ms | 0.685 ms |  1.26 |    0.01 | 3174.88 KB |        2.92 |
| Scrutor       | 500 | 69.47 ms | 0.314 ms | 0.294 ms |  1.15 |    0.01 | 1814.49 KB |        1.67 |
