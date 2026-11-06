```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **27.73 ms** | **1.171 ms** | **1.096 ms** |  **1.00** |    **0.05** |  **116.65 KB** |        **1.00** |
| CseGenerated  | 50  | 27.83 ms | 0.453 ms | 0.424 ms |  1.01 |    0.04 |  116.78 KB |        1.00 |
| CseReflection | 50  | 40.58 ms | 0.564 ms | 0.527 ms |  1.47 |    0.06 |  332.37 KB |        2.85 |
| Scrutor       | 50  | 37.36 ms | 0.481 ms | 0.450 ms |  1.35 |    0.05 |  180.84 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **63.40 ms** | **1.423 ms** | **1.331 ms** |  **1.00** |    **0.03** | **1087.08 KB** |        **1.00** |
| CseGenerated  | 500 | 65.55 ms | 0.808 ms | 0.756 ms |  1.03 |    0.02 | 1171.02 KB |        1.08 |
| CseReflection | 500 | 78.49 ms | 0.900 ms | 0.842 ms |  1.24 |    0.03 |  3174.6 KB |        2.92 |
| Scrutor       | 500 | 71.87 ms | 0.686 ms | 0.642 ms |  1.13 |    0.02 | 1718.49 KB |        1.58 |
