```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **21.97 ms** | **0.118 ms** | **0.111 ms** |  **1.00** |    **0.01** |  **116.97 KB** |        **1.00** |
| CseGenerated  | 50  | 23.35 ms | 0.441 ms | 0.412 ms |  1.06 |    0.02 |  116.69 KB |        1.00 |
| CseReflection | 50  | 34.01 ms | 0.323 ms | 0.303 ms |  1.55 |    0.02 |  334.92 KB |        2.86 |
| Scrutor       | 50  | 30.34 ms | 0.217 ms | 0.203 ms |  1.38 |    0.01 |  181.22 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **48.11 ms** | **0.333 ms** | **0.312 ms** |  **1.00** |    **0.01** | **1090.67 KB** |        **1.00** |
| CseGenerated  | 500 | 50.56 ms | 1.023 ms | 0.957 ms |  1.05 |    0.02 |  1063.8 KB |        0.98 |
| CseReflection | 500 | 62.68 ms | 0.382 ms | 0.357 ms |  1.30 |    0.01 | 3187.77 KB |        2.92 |
| Scrutor       | 500 | 55.92 ms | 0.592 ms | 0.554 ms |  1.16 |    0.01 | 1818.33 KB |        1.67 |
