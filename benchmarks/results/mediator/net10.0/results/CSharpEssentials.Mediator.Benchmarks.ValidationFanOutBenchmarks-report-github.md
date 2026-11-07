```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   | **255.5 ns** | **1.08 ns** | **1.01 ns** |  **1.00** |    **0.01** | **0.0277** |     **464 B** |        **1.00** |
| Cse                 | One    | Valid   | 298.8 ns | 1.13 ns | 1.06 ns |  1.17 |    0.01 | 0.0315 |     528 B |        1.14 |
| MediatRHandWritten  | One    | Valid   | 466.0 ns | 5.00 ns | 4.68 ns |  1.82 |    0.02 | 0.0596 |    1000 B |        2.16 |
|                     |        |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** | **547.0 ns** | **3.96 ns** | **3.71 ns** |  **1.00** |    **0.01** | **0.1087** |    **1824 B** |        **1.00** |
| Cse                 | One    | Invalid | 684.8 ns | 3.87 ns | 3.43 ns |  1.25 |    0.01 | 0.1087 |    1824 B |        1.00 |
| MediatRHandWritten  | One    | Invalid | 783.7 ns | 7.49 ns | 6.26 ns |  1.43 |    0.01 | 0.1345 |    2256 B |        1.24 |
|                     |        |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   | **234.8 ns** | **1.43 ns** | **1.34 ns** |  **1.00** |    **0.01** | **0.0277** |     **464 B** |        **1.00** |
| Cse                 | Two    | Valid   | 276.4 ns | 1.40 ns | 1.31 ns |  1.18 |    0.01 | 0.0315 |     528 B |        1.14 |
| MediatRHandWritten  | Two    | Valid   | 402.6 ns | 3.63 ns | 2.83 ns |  1.71 |    0.01 | 0.0558 |     936 B |        2.02 |
|                     |        |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** | **638.1 ns** | **4.08 ns** | **3.81 ns** |  **1.00** |    **0.01** | **0.1125** |    **1888 B** |        **1.00** |
| Cse                 | Two    | Invalid | 725.2 ns | 3.78 ns | 3.54 ns |  1.14 |    0.01 | 0.1125 |    1888 B |        1.00 |
| MediatRHandWritten  | Two    | Invalid | 851.5 ns | 5.30 ns | 4.70 ns |  1.33 |    0.01 | 0.1373 |    2304 B |        1.22 |
