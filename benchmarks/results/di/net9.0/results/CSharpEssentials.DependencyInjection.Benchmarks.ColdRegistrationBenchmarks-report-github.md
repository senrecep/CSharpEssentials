```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host] : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  cold   : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **21.89 ms** | **0.382 ms** | **0.357 ms** |  **1.00** |    **0.02** |  **116.64 KB** |        **1.00** |
| CseGenerated  | 50  | 23.28 ms | 0.525 ms | 0.491 ms |  1.06 |    0.03 |  116.03 KB |        0.99 |
| CseReflection | 50  | 31.07 ms | 0.252 ms | 0.236 ms |  1.42 |    0.02 |  254.14 KB |        2.18 |
| Scrutor       | 50  | 30.17 ms | 0.313 ms | 0.292 ms |  1.38 |    0.02 |  180.89 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **47.97 ms** | **0.568 ms** | **0.532 ms** |  **1.00** |    **0.01** | **1090.39 KB** |        **1.00** |
| CseGenerated  | 500 | 49.94 ms | 0.795 ms | 0.743 ms |  1.04 |    0.02 | 1111.52 KB |        1.02 |
| CseReflection | 500 | 57.43 ms | 0.594 ms | 0.556 ms |  1.20 |    0.02 | 2379.25 KB |        2.18 |
| Scrutor       | 500 | 55.59 ms | 0.623 ms | 0.583 ms |  1.16 |    0.02 | 1818.05 KB |        1.67 |
