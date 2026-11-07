```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host] : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  cold   : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **25.08 ms** | **0.541 ms** | **0.506 ms** |  **1.00** |    **0.03** |   **117.4 KB** |        **1.00** |
| CseGenerated  | 50  | 26.35 ms | 0.228 ms | 0.213 ms |  1.05 |    0.02 |  116.84 KB |        1.00 |
| CseReflection | 50  | 35.46 ms | 0.279 ms | 0.261 ms |  1.41 |    0.03 |  255.02 KB |        2.17 |
| Scrutor       | 50  | 34.53 ms | 0.525 ms | 0.491 ms |  1.38 |    0.03 |  181.77 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **56.53 ms** | **0.720 ms** | **0.674 ms** |  **1.00** |    **0.02** | **1094.62 KB** |        **1.00** |
| CseGenerated  | 500 | 58.71 ms | 0.807 ms | 0.755 ms |  1.04 |    0.02 | 1067.09 KB |        0.97 |
| CseReflection | 500 | 67.22 ms | 1.176 ms | 1.100 ms |  1.19 |    0.02 | 2482.84 KB |        2.27 |
| Scrutor       | 500 | 64.80 ms | 0.749 ms | 0.700 ms |  1.15 |    0.02 |  1826.2 KB |        1.67 |
