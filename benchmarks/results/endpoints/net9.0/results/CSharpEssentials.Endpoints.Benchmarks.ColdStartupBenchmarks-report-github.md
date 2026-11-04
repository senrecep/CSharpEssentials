```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                 | N   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|----------------------- |---- |-----------:|---------:|---------:|------:|--------:|----------:|----------:|----------:|------------:|
| **Plain**                  | **10**  |   **163.3 ms** |  **8.61 ms** |  **8.05 ms** |  **1.00** |    **0.06** |         **-** |         **-** |      **1 MB** |        **1.00** |
| CseGenerated           | 10  |   165.3 ms |  1.56 ms |  1.46 ms |  1.01 |    0.04 |         - |         - |   1.02 MB |        1.02 |
| CseReflection          | 10  |   169.1 ms |  1.17 ms |  1.09 ms |  1.04 |    0.04 |         - |         - |   1.01 MB |        1.00 |
| FastEndpoints          | 10  |   208.6 ms |  2.71 ms |  2.54 ms |  1.28 |    0.05 |         - |         - |   1.39 MB |        1.38 |
| FastEndpointsSourceGen | 10  |   206.3 ms |  2.10 ms |  1.96 ms |  1.27 |    0.05 |         - |         - |   1.26 MB |        1.25 |
|                        |     |            |          |          |       |         |           |           |           |             |
| **Plain**                  | **100** |   **308.3 ms** |  **1.77 ms** |  **1.65 ms** |  **1.00** |    **0.01** |         **-** |         **-** |   **4.76 MB** |        **1.00** |
| CseGenerated           | 100 |   318.5 ms |  2.81 ms |  2.63 ms |  1.03 |    0.01 |         - |         - |   4.98 MB |        1.05 |
| CseReflection          | 100 |   323.6 ms |  1.79 ms |  1.67 ms |  1.05 |    0.01 |         - |         - |   5.39 MB |        1.13 |
| FastEndpoints          | 100 |   283.8 ms |  2.11 ms |  1.97 ms |  0.92 |    0.01 |         - |         - |   6.05 MB |        1.27 |
| FastEndpointsSourceGen | 100 |   281.7 ms |  2.03 ms |  1.89 ms |  0.91 |    0.01 |         - |         - |   5.96 MB |        1.25 |
|                        |     |            |          |          |       |         |           |           |           |             |
| **Plain**                  | **500** |   **944.1 ms** |  **3.76 ms** |  **3.52 ms** |  **1.00** |    **0.01** | **1000.0000** |         **-** |  **21.43 MB** |        **1.00** |
| CseGenerated           | 500 | 1,010.1 ms |  5.90 ms |  5.52 ms |  1.07 |    0.01 | 1000.0000 |         - |  22.34 MB |        1.04 |
| CseReflection          | 500 | 1,016.2 ms |  4.63 ms |  4.34 ms |  1.08 |    0.01 | 1000.0000 |         - |  23.75 MB |        1.11 |
| FastEndpoints          | 500 |   641.7 ms | 12.55 ms | 11.74 ms |  0.68 |    0.01 | 2000.0000 | 1000.0000 |  36.38 MB |        1.70 |
| FastEndpointsSourceGen | 500 |   641.9 ms |  9.95 ms |  9.31 ms |  0.68 |    0.01 | 2000.0000 | 1000.0000 |  36.47 MB |        1.70 |
