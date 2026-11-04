```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  cold   : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **24.88 ms** | **0.564 ms** | **0.527 ms** |  **1.00** |    **0.03** |  **117.07 KB** |        **1.00** |
| CseGenerated  | 50  | 26.24 ms | 0.325 ms | 0.304 ms |  1.06 |    0.02 |  116.84 KB |        1.00 |
| CseReflection | 50  | 38.21 ms | 0.461 ms | 0.431 ms |  1.54 |    0.03 |  335.52 KB |        2.87 |
| Scrutor       | 50  | 34.75 ms | 1.915 ms | 1.791 ms |  1.40 |    0.07 |   181.4 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **55.70 ms** | **0.663 ms** | **0.620 ms** |  **1.00** |    **0.02** | **1190.34 KB** |        **1.00** |
| CseGenerated  | 500 | 57.83 ms | 0.303 ms | 0.283 ms |  1.04 |    0.01 | 1019.41 KB |        0.86 |
| CseReflection | 500 | 71.20 ms | 0.758 ms | 0.709 ms |  1.28 |    0.02 | 3195.63 KB |        2.68 |
| Scrutor       | 500 | 63.89 ms | 0.658 ms | 0.616 ms |  1.15 |    0.02 | 1729.87 KB |        1.45 |
