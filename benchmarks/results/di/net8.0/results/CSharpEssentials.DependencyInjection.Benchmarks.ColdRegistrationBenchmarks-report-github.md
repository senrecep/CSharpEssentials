```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  cold   : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method        | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------- |---- |---------:|---------:|---------:|------:|--------:|-----------:|------------:|
| **Manual**        | **50**  | **22.38 ms** | **0.157 ms** | **0.147 ms** |  **1.00** |    **0.01** |  **117.04 KB** |        **1.00** |
| CseGenerated  | 50  | 23.76 ms | 0.282 ms | 0.264 ms |  1.06 |    0.01 |  116.76 KB |        1.00 |
| CseReflection | 50  | 36.24 ms | 0.311 ms | 0.291 ms |  1.62 |    0.02 |  403.36 KB |        3.45 |
| Scrutor       | 50  | 31.93 ms | 0.544 ms | 0.509 ms |  1.43 |    0.02 |   181.3 KB |        1.55 |
|               |     |          |          |          |       |         |            |             |
| **Manual**        | **500** | **50.20 ms** | **0.260 ms** | **0.243 ms** |  **1.00** |    **0.01** | **1091.45 KB** |        **1.00** |
| CseGenerated  | 500 | 52.65 ms | 0.457 ms | 0.427 ms |  1.05 |    0.01 | 1016.52 KB |        0.93 |
| CseReflection | 500 | 67.78 ms | 0.792 ms | 0.741 ms |  1.35 |    0.02 | 3871.52 KB |        3.55 |
| Scrutor       | 500 | 58.33 ms | 0.550 ms | 0.515 ms |  1.16 |    0.01 | 1819.11 KB |        1.67 |
