```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   |   **315.9 ns** |  **1.65 ns** |  **1.55 ns** |  **1.00** |    **0.01** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | One    | Valid   |   366.4 ns |  5.02 ns |  4.69 ns |  1.16 |    0.02 | 0.0353 |     592 B |        1.00 |
| MediatRHandWritten  | One    | Valid   |   595.1 ns |  3.93 ns |  3.28 ns |  1.88 |    0.01 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** |   **926.1 ns** |  **4.07 ns** |  **3.81 ns** |  **1.00** |    **0.01** | **0.1249** |    **2096 B** |        **1.00** |
| Cse                 | One    | Invalid | 1,093.5 ns | 13.32 ns | 12.46 ns |  1.18 |    0.01 | 0.1240 |    2096 B |        1.00 |
| MediatRHandWritten  | One    | Invalid | 1,077.6 ns |  5.09 ns |  4.52 ns |  1.16 |    0.01 | 0.1507 |    2528 B |        1.21 |
|                     |        |         |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   |   **300.9 ns** |  **3.04 ns** |  **2.84 ns** |  **1.00** |    **0.01** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | Two    | Valid   |   380.1 ns |  1.85 ns |  1.64 ns |  1.26 |    0.01 | 0.0353 |     592 B |        1.00 |
| MediatRHandWritten  | Two    | Valid   |   621.2 ns |  3.50 ns |  3.28 ns |  2.06 |    0.02 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** |   **946.7 ns** | **11.27 ns** |  **9.99 ns** |  **1.00** |    **0.01** | **0.1249** |    **2096 B** |        **1.00** |
| Cse                 | Two    | Invalid | 1,111.6 ns |  8.13 ns |  7.60 ns |  1.17 |    0.01 | 0.1240 |    2096 B |        1.00 |
| MediatRHandWritten  | Two    | Invalid | 1,069.9 ns |  5.46 ns |  4.84 ns |  1.13 |    0.01 | 0.1507 |    2528 B |        1.21 |
