```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **21.90 ms** | **0.538 ms** | **0.503 ms** |  **1.00** |    **0.03** |  **116.31 KB** |        **1.00** |
| CseGenerated  | 50  | 22.95 ms | 0.262 ms | 0.245 ms |  1.05 |    0.02 |  116.03 KB |        1.00 |
| CseReflection | 50  | 33.73 ms | 0.415 ms | 0.388 ms |  1.54 |    0.04 |  334.59 KB |        2.88 |
| Scrutor       | 50  | 30.12 ms | 0.511 ms | 0.478 ms |  1.38 |    0.04 |  181.22 KB |        1.56 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **47.51 ms** | **0.513 ms** | **0.480 ms** |  **1.00** |    **0.01** | **1090.67 KB** |        **1.00** |
| CseGenerated  | 500 | 49.59 ms | 0.461 ms | 0.431 ms |  1.04 |    0.01 | 1063.52 KB |        0.98 |
| CseReflection | 500 | 61.98 ms | 0.712 ms | 0.666 ms |  1.30 |    0.02 | 3284.09 KB |        3.01 |
| Scrutor       | 500 | 55.01 ms | 0.571 ms | 0.534 ms |  1.16 |    0.02 | 1818.33 KB |        1.67 |
