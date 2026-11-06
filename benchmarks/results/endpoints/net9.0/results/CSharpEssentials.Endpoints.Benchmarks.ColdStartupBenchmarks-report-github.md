```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                 | N   | Mean       | Error    | StdDev   | Ratio | Gen0      | Gen1      | Allocated | Alloc Ratio |
|----------------------- |---- |-----------:|---------:|---------:|------:|----------:|----------:|----------:|------------:|
| **Plain**                  | **10**  |   **164.1 ms** |  **1.18 ms** |  **1.10 ms** |  **1.00** |         **-** |         **-** |    **1.1 MB** |        **1.00** |
| CseGenerated           | 10  |   167.9 ms |  1.29 ms |  1.20 ms |  1.02 |         - |         - |   1.11 MB |        1.01 |
| CseReflection          | 10  |   172.3 ms |  1.01 ms |  0.95 ms |  1.05 |         - |         - |   1.02 MB |        0.93 |
| FastEndpoints          | 10  |   211.1 ms |  1.53 ms |  1.43 ms |  1.29 |         - |         - |   1.18 MB |        1.07 |
| FastEndpointsSourceGen | 10  |   210.1 ms |  1.74 ms |  1.63 ms |  1.28 |         - |         - |   1.18 MB |        1.07 |
|                        |     |            |          |          |       |           |           |           |             |
| **Plain**                  | **100** |   **313.4 ms** |  **1.55 ms** |  **1.45 ms** |  **1.00** |         **-** |         **-** |   **4.71 MB** |        **1.00** |
| CseGenerated           | 100 |   321.2 ms |  1.50 ms |  1.40 ms |  1.03 |         - |         - |   4.98 MB |        1.06 |
| CseReflection          | 100 |   326.7 ms |  1.95 ms |  1.82 ms |  1.04 |         - |         - |   5.28 MB |        1.12 |
| FastEndpoints          | 100 |   288.4 ms |  1.22 ms |  1.14 ms |  0.92 |         - |         - |   6.17 MB |        1.31 |
| FastEndpointsSourceGen | 100 |   287.5 ms |  2.13 ms |  1.99 ms |  0.92 |         - |         - |   6.06 MB |        1.29 |
|                        |     |            |          |          |       |           |           |           |             |
| **Plain**                  | **500** |   **946.8 ms** |  **2.74 ms** |  **2.56 ms** |  **1.00** | **1000.0000** |         **-** |  **21.53 MB** |        **1.00** |
| CseGenerated           | 500 | 1,013.5 ms |  2.14 ms |  2.00 ms |  1.07 | 1000.0000 |         - |  22.36 MB |        1.04 |
| CseReflection          | 500 | 1,022.1 ms |  7.13 ms |  6.67 ms |  1.08 | 1000.0000 |         - |  23.76 MB |        1.10 |
| FastEndpoints          | 500 |   643.4 ms | 10.98 ms | 10.28 ms |  0.68 | 2000.0000 | 1000.0000 |   36.4 MB |        1.69 |
| FastEndpointsSourceGen | 500 |   636.4 ms | 10.33 ms |  9.67 ms |  0.67 | 2000.0000 | 1000.0000 |  36.35 MB |        1.69 |
