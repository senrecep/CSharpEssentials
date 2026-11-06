```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   |   **291.3 ns** | **2.34 ns** | **2.19 ns** |  **1.00** |    **0.01** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | One    | Valid   |   414.0 ns | 5.04 ns | 4.72 ns |  1.42 |    0.02 | 0.0439 |     736 B |        1.24 |
| MediatRHandWritten  | One    | Valid   |   515.7 ns | 1.23 ns | 1.15 ns |  1.77 |    0.01 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** |   **705.1 ns** | **5.25 ns** | **4.91 ns** |  **1.00** |    **0.01** | **0.1192** |    **2000 B** |        **1.00** |
| Cse                 | One    | Invalid | 1,273.4 ns | 4.93 ns | 4.11 ns |  1.81 |    0.01 | 0.1488 |    2496 B |        1.25 |
| MediatRHandWritten  | One    | Invalid |   905.6 ns | 8.24 ns | 7.71 ns |  1.28 |    0.01 | 0.1450 |    2432 B |        1.22 |
|                     |        |         |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   |   **284.2 ns** | **0.73 ns** | **0.61 ns** |  **1.00** |    **0.00** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | Two    | Valid   |   448.3 ns | 3.32 ns | 3.11 ns |  1.58 |    0.01 | 0.0496 |     832 B |        1.41 |
| MediatRHandWritten  | Two    | Valid   |   549.4 ns | 2.51 ns | 2.34 ns |  1.93 |    0.01 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** |   **710.8 ns** | **4.68 ns** | **4.38 ns** |  **1.00** |    **0.01** | **0.1192** |    **2000 B** |        **1.00** |
| Cse                 | Two    | Invalid | 1,346.6 ns | 4.87 ns | 4.55 ns |  1.89 |    0.01 | 0.1545 |    2592 B |        1.30 |
| MediatRHandWritten  | Two    | Invalid |   983.3 ns | 5.36 ns | 5.01 ns |  1.38 |    0.01 | 0.1411 |    2384 B |        1.19 |
