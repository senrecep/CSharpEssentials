```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   |   **309.5 ns** | **1.19 ns** | **1.05 ns** |  **1.00** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | One    | Valid   |   520.5 ns | 2.11 ns | 1.98 ns |  1.68 | 0.0439 |     736 B |        1.24 |
| MediatRHandWritten  | One    | Valid   |   613.6 ns | 2.41 ns | 2.26 ns |  1.98 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |            |         |         |       |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** |   **933.1 ns** | **5.96 ns** | **5.57 ns** |  **1.00** | **0.1249** |    **2096 B** |        **1.00** |
| Cse                 | One    | Invalid | 1,509.4 ns | 5.64 ns | 5.28 ns |  1.62 | 0.1564 |    2640 B |        1.26 |
| MediatRHandWritten  | One    | Invalid | 1,106.6 ns | 3.89 ns | 3.45 ns |  1.19 | 0.1507 |    2528 B |        1.21 |
|                     |        |         |            |         |         |       |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   |   **322.0 ns** | **1.17 ns** | **1.10 ns** |  **1.00** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | Two    | Valid   |   599.8 ns | 2.44 ns | 2.28 ns |  1.86 | 0.0496 |     832 B |        1.41 |
| MediatRHandWritten  | Two    | Valid   |   572.0 ns | 3.23 ns | 3.03 ns |  1.78 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |            |         |         |       |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** |   **938.6 ns** | **6.38 ns** | **5.33 ns** |  **1.00** | **0.1249** |    **2096 B** |        **1.00** |
| Cse                 | Two    | Invalid | 1,726.1 ns | 9.41 ns | 8.34 ns |  1.84 | 0.1621 |    2736 B |        1.31 |
| MediatRHandWritten  | Two    | Invalid | 1,160.8 ns | 3.34 ns | 3.13 ns |  1.24 | 0.1507 |    2528 B |        1.21 |
