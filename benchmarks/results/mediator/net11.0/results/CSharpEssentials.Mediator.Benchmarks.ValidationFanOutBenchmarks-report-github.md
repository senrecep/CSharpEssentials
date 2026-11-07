```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   | **239.9 ns** | **1.54 ns** | **1.44 ns** |  **1.00** | **0.0272** |     **456 B** |        **1.00** |
| Cse                 | One    | Valid   | 294.8 ns | 1.49 ns | 1.32 ns |  1.23 | 0.0305 |     512 B |        1.12 |
| MediatRHandWritten  | One    | Valid   | 408.0 ns | 1.07 ns | 1.01 ns |  1.70 | 0.0577 |     968 B |        2.12 |
|                     |        |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** | **599.8 ns** | **4.00 ns** | **3.74 ns** |  **1.00** | **0.1116** |    **1872 B** |        **1.00** |
| Cse                 | One    | Invalid | 674.8 ns | 3.26 ns | 2.89 ns |  1.13 | 0.1078 |    1816 B |        0.97 |
| MediatRHandWritten  | One    | Invalid | 776.7 ns | 6.63 ns | 6.20 ns |  1.29 | 0.1354 |    2272 B |        1.21 |
|                     |        |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   | **244.7 ns** | **1.24 ns** | **1.16 ns** |  **1.00** | **0.0305** |     **512 B** |        **1.00** |
| Cse                 | Two    | Valid   | 283.3 ns | 3.15 ns | 2.79 ns |  1.16 | 0.0272 |     456 B |        0.89 |
| MediatRHandWritten  | Two    | Valid   | 418.6 ns | 1.60 ns | 1.50 ns |  1.71 | 0.0544 |     912 B |        1.78 |
|                     |        |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** | **506.2 ns** | **1.59 ns** | **1.33 ns** |  **1.00** | **0.1078** |    **1816 B** |        **1.00** |
| Cse                 | Two    | Invalid | 578.8 ns | 3.01 ns | 2.82 ns |  1.14 | 0.1049 |    1760 B |        0.97 |
| MediatRHandWritten  | Two    | Invalid | 670.8 ns | 4.17 ns | 3.90 ns |  1.33 | 0.1354 |    2272 B |        1.25 |
