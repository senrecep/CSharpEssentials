```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   |   **258.2 ns** | **1.24 ns** | **1.16 ns** |  **1.00** |    **0.01** | **0.0315** |     **528 B** |        **1.00** |
| Cse                 | One    | Valid   |   354.7 ns | 1.58 ns | 1.40 ns |  1.37 |    0.01 | 0.0362 |     608 B |        1.15 |
| MediatRHandWritten  | One    | Valid   |   450.7 ns | 2.31 ns | 2.16 ns |  1.75 |    0.01 | 0.0558 |     936 B |        1.77 |
|                     |        |         |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** |   **598.9 ns** | **4.42 ns** | **3.69 ns** |  **1.00** |    **0.01** | **0.1154** |    **1936 B** |        **1.00** |
| Cse                 | One    | Invalid | 1,157.9 ns | 6.95 ns | 6.51 ns |  1.93 |    0.02 | 0.1450 |    2432 B |        1.26 |
| MediatRHandWritten  | One    | Invalid |   821.5 ns | 5.34 ns | 4.17 ns |  1.37 |    0.01 | 0.1345 |    2256 B |        1.17 |
|                     |        |         |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   |   **263.2 ns** | **1.13 ns** | **1.06 ns** |  **1.00** |    **0.01** | **0.0315** |     **528 B** |        **1.00** |
| Cse                 | Two    | Valid   |   419.5 ns | 1.50 ns | 1.40 ns |  1.59 |    0.01 | 0.0420 |     704 B |        1.33 |
| MediatRHandWritten  | Two    | Valid   |   454.1 ns | 3.57 ns | 3.34 ns |  1.73 |    0.01 | 0.0558 |     936 B |        1.77 |
|                     |        |         |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** |   **696.8 ns** | **4.29 ns** | **3.80 ns** |  **1.00** |    **0.01** | **0.1154** |    **1936 B** |        **1.00** |
| Cse                 | Two    | Invalid | 1,233.0 ns | 2.04 ns | 1.70 ns |  1.77 |    0.01 | 0.1469 |    2464 B |        1.27 |
| MediatRHandWritten  | Two    | Invalid |   773.9 ns | 8.90 ns | 7.89 ns |  1.11 |    0.01 | 0.1335 |    2240 B |        1.16 |
