```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method      | Variant              | Mean     | Error    | StdDev   | Gen0   | Exceptions | Allocated |
|------------ |--------------------- |---------:|---------:|---------:|-------:|-----------:|----------:|
| **GetById**     | **Plain**                | **12.86 μs** | **0.253 μs** | **0.237 μs** | **0.6104** |          **-** |  **10.11 KB** |
| PostValid   | Plain                | 19.88 μs | 0.354 μs | 0.379 μs | 0.7324 |          - |  13.51 KB |
| PostInvalid | Plain                | 26.07 μs | 0.485 μs | 0.680 μs | 0.9766 |          - |  16.99 KB |
| Delete      | Plain                | 10.77 μs | 0.212 μs | 0.305 μs | 0.5493 |          - |   9.31 KB |
| **GetById**     | **CseGenerated**         | **12.82 μs** | **0.245 μs** | **0.310 μs** | **0.6104** |          **-** |  **10.11 KB** |
| PostValid   | CseGenerated         | 20.02 μs | 0.398 μs | 0.785 μs | 0.7324 |          - |  13.52 KB |
| PostInvalid | CseGenerated         | 25.97 μs | 0.375 μs | 0.350 μs | 0.9766 |          - |  16.99 KB |
| Delete      | CseGenerated         | 10.37 μs | 0.206 μs | 0.430 μs | 0.4883 |          - |   9.32 KB |
| **GetById**     | **CseReflection**        | **12.64 μs** | **0.249 μs** | **0.245 μs** | **0.6104** |          **-** |  **10.11 KB** |
| PostValid   | CseReflection        | 18.44 μs | 0.369 μs | 0.540 μs | 0.7324 |          - |   13.5 KB |
| PostInvalid | CseReflection        | 27.11 μs | 0.527 μs | 0.772 μs | 0.9766 |          - |  16.99 KB |
| Delete      | CseReflection        | 10.97 μs | 0.212 μs | 0.268 μs | 0.5493 |          - |   9.33 KB |
| **GetById**     | **Carter**               | **12.75 μs** | **0.242 μs** | **0.269 μs** | **0.6104** |          **-** |  **10.11 KB** |
| PostValid   | Carter               | 18.82 μs | 0.374 μs | 0.582 μs | 0.7324 |          - |  12.71 KB |
| PostInvalid | Carter               | 33.01 μs | 0.638 μs | 0.873 μs | 1.2207 |          - |  22.68 KB |
| Delete      | Carter               | 10.66 μs | 0.211 μs | 0.370 μs | 0.4883 |          - |   9.31 KB |
| **GetById**     | **FastEndpoints**        | **13.70 μs** | **0.211 μs** | **0.243 μs** | **0.6104** |          **-** |  **11.48 KB** |
| PostValid   | FastEndpoints        | 24.09 μs | 0.477 μs | 1.047 μs | 0.7324 |          - |  14.12 KB |
| PostInvalid | FastEndpoints        | 44.15 μs | 0.648 μs | 0.575 μs | 1.4648 |     2.0000 |   24.6 KB |
| Delete      | FastEndpoints        | 13.23 μs | 0.263 μs | 0.292 μs | 0.6104 |          - |  10.51 KB |
| **GetById**     | **FastE(...)ceGen [22]** | **13.92 μs** | **0.277 μs** | **0.319 μs** | **0.6104** |          **-** |  **11.49 KB** |
| PostValid   | FastE(...)ceGen [22] | 21.11 μs | 0.380 μs | 0.317 μs | 0.7324 |          - |  14.07 KB |
| PostInvalid | FastE(...)ceGen [22] | 45.16 μs | 0.871 μs | 1.070 μs | 1.4648 |     2.0000 |   24.6 KB |
| Delete      | FastE(...)ceGen [22] | 13.35 μs | 0.258 μs | 0.265 μs | 0.6104 |          - |  10.48 KB |
