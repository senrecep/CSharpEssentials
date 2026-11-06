```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  cold   : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                 | N   | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0      | Gen1      | Allocated   | Alloc Ratio |
|----------------------- |---- |---------:|---------:|---------:|------:|--------:|----------:|----------:|------------:|------------:|
| **Plain**                  | **10**  | **172.5 ms** |  **6.18 ms** |  **5.78 ms** |  **1.00** |    **0.05** |         **-** |         **-** |    **978.4 KB** |        **1.00** |
| CseGenerated           | 10  | 171.9 ms |  5.09 ms |  4.76 ms |  1.00 |    0.04 |         - |         - |   1175.2 KB |        1.20 |
| CseReflection          | 10  | 180.7 ms |  5.65 ms |  5.28 ms |  1.05 |    0.05 |         - |         - |  1019.67 KB |        1.04 |
| FastEndpoints          | 10  | 225.6 ms |  6.68 ms |  6.25 ms |  1.31 |    0.06 |         - |         - |  1318.31 KB |        1.35 |
| FastEndpointsSourceGen | 10  | 225.7 ms |  5.13 ms |  4.80 ms |  1.31 |    0.05 |         - |         - |  1295.13 KB |        1.32 |
|                        |     |          |          |          |       |         |           |           |             |             |
| **Plain**                  | **100** | **294.0 ms** |  **5.28 ms** |  **4.94 ms** |  **1.00** |    **0.02** |         **-** |         **-** |  **4841.54 KB** |        **1.00** |
| CseGenerated           | 100 | 301.2 ms |  1.50 ms |  1.41 ms |  1.02 |    0.02 |         - |         - |  5008.13 KB |        1.03 |
| CseReflection          | 100 | 307.7 ms |  1.64 ms |  1.53 ms |  1.05 |    0.02 |         - |         - |  5417.11 KB |        1.12 |
| FastEndpoints          | 100 | 297.7 ms |  1.82 ms |  1.71 ms |  1.01 |    0.02 |         - |         - |  6020.33 KB |        1.24 |
| FastEndpointsSourceGen | 100 | 296.3 ms |  1.79 ms |  1.67 ms |  1.01 |    0.02 |         - |         - |  5976.69 KB |        1.23 |
|                        |     |          |          |          |       |         |           |           |             |             |
| **Plain**                  | **500** | **840.4 ms** |  **4.44 ms** |  **4.15 ms** |  **1.00** |    **0.01** | **1000.0000** |         **-** | **22054.41 KB** |        **1.00** |
| CseGenerated           | 500 | 905.0 ms | 12.23 ms | 11.44 ms |  1.08 |    0.01 | 1000.0000 |         - | 22934.49 KB |        1.04 |
| CseReflection          | 500 | 915.8 ms | 10.23 ms |  9.57 ms |  1.09 |    0.01 | 1000.0000 |         - |  24460.2 KB |        1.11 |
| FastEndpoints          | 500 | 653.2 ms |  5.23 ms |  4.90 ms |  0.78 |    0.01 | 2000.0000 | 1000.0000 | 36192.73 KB |        1.64 |
| FastEndpointsSourceGen | 500 | 652.0 ms |  3.68 ms |  3.44 ms |  0.78 |    0.01 | 2000.0000 | 1000.0000 | 36137.02 KB |        1.64 |
