```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  cold   : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                 | N   | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|----------------------- |---- |-----------:|--------:|--------:|------:|--------:|----------:|----------:|----------:|------------:|
| **Plain**                  | **10**  |   **160.4 ms** | **4.97 ms** | **4.65 ms** |  **1.00** |    **0.04** |         **-** |         **-** |    **1.1 MB** |        **1.00** |
| CseGenerated           | 10  |   164.9 ms | 4.31 ms | 4.03 ms |  1.03 |    0.04 |         - |         - |   1.11 MB |        1.01 |
| CseReflection          | 10  |   169.5 ms | 4.08 ms | 3.81 ms |  1.06 |    0.04 |         - |         - |   1.05 MB |        0.96 |
| Carter                 | 10  |   167.6 ms | 4.00 ms | 3.74 ms |  1.05 |    0.04 |         - |         - |   1.09 MB |        0.99 |
| FastEndpoints          | 10  |   213.5 ms | 5.33 ms | 4.99 ms |  1.33 |    0.05 |         - |         - |   1.18 MB |        1.08 |
| FastEndpointsSourceGen | 10  |   205.5 ms | 1.58 ms | 1.48 ms |  1.28 |    0.04 |         - |         - |   1.15 MB |        1.05 |
|                        |     |            |         |         |       |         |           |           |           |             |
| **Plain**                  | **100** |   **323.2 ms** | **3.27 ms** | **3.06 ms** |  **1.00** |    **0.01** |         **-** |         **-** |   **4.68 MB** |        **1.00** |
| CseGenerated           | 100 |   339.6 ms | 3.92 ms | 3.67 ms |  1.05 |    0.01 |         - |         - |   4.95 MB |        1.06 |
| CseReflection          | 100 |   340.9 ms | 1.63 ms | 1.52 ms |  1.05 |    0.01 |         - |         - |   5.24 MB |        1.12 |
| Carter                 | 100 |   338.0 ms | 1.67 ms | 1.56 ms |  1.05 |    0.01 |         - |         - |   4.93 MB |        1.05 |
| FastEndpoints          | 100 |   296.7 ms | 1.39 ms | 1.30 ms |  0.92 |    0.01 |         - |         - |    6.2 MB |        1.32 |
| FastEndpointsSourceGen | 100 |   301.2 ms | 7.81 ms | 7.31 ms |  0.93 |    0.02 |         - |         - |   6.15 MB |        1.31 |
|                        |     |            |         |         |       |         |           |           |           |             |
| **Plain**                  | **500** | **1,041.8 ms** | **5.42 ms** | **5.07 ms** |  **1.00** |    **0.01** | **1000.0000** |         **-** |  **21.26 MB** |        **1.00** |
| CseGenerated           | 500 | 1,116.3 ms | 6.15 ms | 5.75 ms |  1.07 |    0.01 | 1000.0000 |         - |   22.4 MB |        1.05 |
| CseReflection          | 500 | 1,123.2 ms | 6.10 ms | 5.70 ms |  1.08 |    0.01 | 1000.0000 |         - |  23.62 MB |        1.11 |
| Carter                 | 500 | 1,108.0 ms | 4.48 ms | 4.19 ms |  1.06 |    0.01 | 1000.0000 |         - |  22.37 MB |        1.05 |
| FastEndpoints          | 500 |   688.4 ms | 3.20 ms | 2.99 ms |  0.66 |    0.00 | 2000.0000 | 1000.0000 |  35.97 MB |        1.69 |
| FastEndpointsSourceGen | 500 |   690.4 ms | 5.30 ms | 4.95 ms |  0.66 |    0.01 | 2000.0000 | 1000.0000 |  35.87 MB |        1.69 |
