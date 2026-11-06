```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                 | N   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|----------------------- |---- |-----------:|---------:|---------:|------:|--------:|----------:|----------:|----------:|------------:|
| **Plain**                  | **10**  |   **162.6 ms** |  **3.35 ms** |  **3.13 ms** |  **1.00** |    **0.03** |         **-** |         **-** |   **1.02 MB** |        **1.00** |
| CseGenerated           | 10  |   167.4 ms |  3.43 ms |  3.21 ms |  1.03 |    0.03 |         - |         - |   1.01 MB |        0.99 |
| CseReflection          | 10  |   174.2 ms |  4.03 ms |  3.77 ms |  1.07 |    0.03 |         - |         - |   1.18 MB |        1.17 |
| Carter                 | 10  |   171.7 ms |  3.88 ms |  3.63 ms |  1.06 |    0.03 |         - |         - |   1.09 MB |        1.08 |
| FastEndpoints          | 10  |   215.3 ms |  4.47 ms |  4.18 ms |  1.32 |    0.03 |         - |         - |    1.3 MB |        1.28 |
| FastEndpointsSourceGen | 10  |   212.2 ms |  5.40 ms |  5.05 ms |  1.31 |    0.04 |         - |         - |    1.2 MB |        1.19 |
|                        |     |            |          |          |       |         |           |           |           |             |
| **Plain**                  | **100** |   **345.4 ms** |  **8.71 ms** |  **8.15 ms** |  **1.00** |    **0.03** |         **-** |         **-** |   **4.88 MB** |        **1.00** |
| CseGenerated           | 100 |   359.2 ms |  4.26 ms |  3.99 ms |  1.04 |    0.03 |         - |         - |   5.11 MB |        1.05 |
| CseReflection          | 100 |   357.2 ms |  5.25 ms |  4.91 ms |  1.03 |    0.03 |         - |         - |   5.32 MB |        1.09 |
| Carter                 | 100 |   355.1 ms |  4.89 ms |  4.57 ms |  1.03 |    0.03 |         - |         - |   5.01 MB |        1.03 |
| FastEndpoints          | 100 |   299.8 ms |  4.58 ms |  4.29 ms |  0.87 |    0.02 |         - |         - |   6.17 MB |        1.26 |
| FastEndpointsSourceGen | 100 |   299.3 ms |  6.24 ms |  5.83 ms |  0.87 |    0.03 |         - |         - |   6.17 MB |        1.26 |
|                        |     |            |          |          |       |         |           |           |           |             |
| **Plain**                  | **500** | **1,116.1 ms** | **12.55 ms** | **11.74 ms** |  **1.00** |    **0.01** | **1000.0000** |         **-** |  **22.22 MB** |        **1.00** |
| CseGenerated           | 500 | 1,186.6 ms |  9.64 ms |  9.01 ms |  1.06 |    0.01 | 1000.0000 |         - |  22.98 MB |        1.03 |
| CseReflection          | 500 | 1,207.9 ms | 13.62 ms | 12.74 ms |  1.08 |    0.02 | 1000.0000 |         - |  24.53 MB |        1.10 |
| Carter                 | 500 | 1,185.1 ms | 12.96 ms | 12.12 ms |  1.06 |    0.02 | 1000.0000 |         - |  22.69 MB |        1.02 |
| FastEndpoints          | 500 |   687.2 ms |  8.96 ms |  8.38 ms |  0.62 |    0.01 | 2000.0000 | 1000.0000 |  36.89 MB |        1.66 |
| FastEndpointsSourceGen | 500 |   689.1 ms |  9.01 ms |  8.42 ms |  0.62 |    0.01 | 2000.0000 | 1000.0000 |  36.92 MB |        1.66 |
