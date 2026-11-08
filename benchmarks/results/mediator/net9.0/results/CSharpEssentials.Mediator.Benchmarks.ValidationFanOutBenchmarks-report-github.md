```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method              | Groups | Input   | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |------- |-------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **One**    | **Valid**   | **297.8 ns** | **1.10 ns** | **0.92 ns** |  **1.00** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | One    | Valid   | 307.7 ns | 1.66 ns | 1.55 ns |  1.03 | 0.0353 |     592 B |        1.00 |
| MediatRHandWritten  | One    | Valid   | 548.5 ns | 2.76 ns | 2.45 ns |  1.84 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **One**    | **Invalid** | **759.6 ns** | **3.97 ns** | **3.72 ns** |  **1.00** | **0.1192** |    **2000 B** |        **1.00** |
| Cse                 | One    | Invalid | 818.2 ns | 7.48 ns | 6.63 ns |  1.08 | 0.1163 |    1952 B |        0.98 |
| MediatRHandWritten  | One    | Invalid | 911.9 ns | 5.82 ns | 5.44 ns |  1.20 | 0.1450 |    2432 B |        1.22 |
|                     |        |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Two**    | **Valid**   | **298.7 ns** | **1.38 ns** | **1.23 ns** |  **1.00** | **0.0353** |     **592 B** |        **1.00** |
| Cse                 | Two    | Valid   | 318.6 ns | 1.66 ns | 1.56 ns |  1.07 | 0.0353 |     592 B |        1.00 |
| MediatRHandWritten  | Two    | Valid   | 495.0 ns | 3.10 ns | 2.90 ns |  1.66 | 0.0668 |    1128 B |        1.91 |
|                     |        |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Two**    | **Invalid** | **692.6 ns** | **2.38 ns** | **2.23 ns** |  **1.00** | **0.1192** |    **2000 B** |        **1.00** |
| Cse                 | Two    | Invalid | 727.1 ns | 4.22 ns | 3.95 ns |  1.05 | 0.1163 |    1952 B |        0.98 |
| MediatRHandWritten  | Two    | Invalid | 947.2 ns | 7.16 ns | 6.70 ns |  1.37 | 0.1421 |    2384 B |        1.19 |
