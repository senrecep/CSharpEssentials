```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   |   **235.2 ns** |  **4.57 ns** |  **4.69 ns** |  **1.00** |    **0.03** | **0.0272** |     **456 B** |        **1.00** |
| Cse                 | One    | Valid   |   384.9 ns |  1.18 ns |  1.11 ns |  1.64 |    0.03 | 0.0358 |     600 B |        1.32 |
| MediatRHandWritten  | One    | Valid   |   437.2 ns |  2.28 ns |  2.02 ns |  1.86 |    0.04 | 0.0544 |     912 B |        2.00 |
|                     |        |         |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** |   **622.3 ns** |  **3.10 ns** |  **2.90 ns** |  **1.00** |    **0.01** | **0.1106** |    **1864 B** |        **1.00** |
| Cse                 | One    | Invalid | 1,103.7 ns |  6.67 ns |  6.24 ns |  1.77 |    0.01 | 0.1392 |    2360 B |        1.27 |
| MediatRHandWritten  | One    | Invalid |   809.3 ns | 15.43 ns | 14.43 ns |  1.30 |    0.02 | 0.1354 |    2272 B |        1.22 |
|                     |        |         |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   |   **234.7 ns** |  **1.00 ns** |  **0.83 ns** |  **1.00** |    **0.00** | **0.0272** |     **456 B** |        **1.00** |
| Cse                 | Two    | Valid   |   401.3 ns |  1.35 ns |  1.20 ns |  1.71 |    0.01 | 0.0448 |     752 B |        1.65 |
| MediatRHandWritten  | Two    | Valid   |   401.5 ns |  3.23 ns |  3.03 ns |  1.71 |    0.01 | 0.0544 |     912 B |        2.00 |
|                     |        |         |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** |   **635.2 ns** |  **2.59 ns** |  **2.02 ns** |  **1.00** |    **0.00** | **0.1144** |    **1920 B** |        **1.00** |
| Cse                 | Two    | Invalid | 1,081.0 ns |  5.21 ns |  4.62 ns |  1.70 |    0.01 | 0.1450 |    2456 B |        1.28 |
| MediatRHandWritten  | Two    | Invalid |   678.0 ns |  4.31 ns |  4.03 ns |  1.07 |    0.01 | 0.1316 |    2216 B |        1.15 |
