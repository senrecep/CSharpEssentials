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
| **Manual**        | **50**  | **25.39 ms** | **0.855 ms** | **0.799 ms** |  **1.00** |    **0.04** |   **117.4 KB** |        **1.00** |
| CseGenerated  | 50  | 26.44 ms | 0.375 ms | 0.351 ms |  1.04 |    0.03 |  116.84 KB |        1.00 |
| CseReflection | 50  | 39.29 ms | 1.481 ms | 1.385 ms |  1.55 |    0.07 |   335.8 KB |        2.86 |
| Scrutor       | 50  | 35.37 ms | 1.814 ms | 1.697 ms |  1.39 |    0.08 |  181.77 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **56.89 ms** | **1.629 ms** | **1.524 ms** |  **1.00** |    **0.04** | **1094.34 KB** |        **1.00** |
| CseGenerated  | 500 | 59.90 ms | 3.004 ms | 2.810 ms |  1.05 |    0.05 | 1019.41 KB |        0.93 |
| CseReflection | 500 | 72.52 ms | 2.230 ms | 2.086 ms |  1.28 |    0.05 | 3291.73 KB |        3.01 |
| Scrutor       | 500 | 65.75 ms | 2.804 ms | 2.623 ms |  1.16 |    0.05 | 1825.91 KB |        1.67 |
