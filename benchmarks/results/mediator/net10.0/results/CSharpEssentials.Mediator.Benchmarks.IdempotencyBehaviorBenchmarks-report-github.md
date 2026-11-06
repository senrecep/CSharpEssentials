```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **554.4 ns** |  **1.93 ns** |  **1.81 ns** |  **1.00** |    **0.00** | **0.0391** |     **664 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   896.9 ns |  3.48 ns |  3.26 ns |  1.62 |    0.01 | 0.0610 |    1032 B |        1.55 |
| MediatRHandWritten  | Replay         | Supplied     |   714.1 ns |  2.70 ns |  2.53 ns |  1.29 |    0.01 | 0.0610 |    1032 B |        1.55 |
|                     |                |              |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **1,817.2 ns** |  **9.38 ns** |  **8.77 ns** |  **1.00** |    **0.01** | **0.0591** |    **1000 B** |        **1.00** |
| Cse                 | Replay         | Default      | 1,943.5 ns |  8.35 ns |  6.97 ns |  1.07 |    0.01 | 0.0687 |    1184 B |        1.18 |
| MediatRHandWritten  | Replay         | Default      | 2,018.4 ns | 16.86 ns | 15.77 ns |  1.11 |    0.01 | 0.0801 |    1368 B |        1.37 |
|                     |                |              |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **4,916.2 ns** | **18.23 ns** | **17.05 ns** |  **1.00** |    **0.00** | **0.2975** |    **5096 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 4,796.3 ns | 15.93 ns | 13.30 ns |  0.98 |    0.00 | 0.0687 |    1184 B |        0.23 |
| MediatRHandWritten  | Replay         | DefaultLarge | 5,081.2 ns | 17.74 ns | 14.81 ns |  1.03 |    0.00 | 0.3204 |    5464 B |        1.07 |
|                     |                |              |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **300.7 ns** |  **0.91 ns** |  **0.85 ns** |  **1.00** |    **0.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     | 1,627.9 ns |  7.27 ns |  6.80 ns |  5.41 |    0.03 | 0.0381 |     648 B |        4.50 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   447.3 ns |  1.89 ns |  1.67 ns |  1.49 |    0.01 | 0.0367 |     616 B |        4.28 |
|                     |                |              |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,331.1 ns** |  **4.72 ns** |  **4.18 ns** |  **1.00** |    **0.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 2,658.2 ns |  3.01 ns |  2.81 ns |  2.00 |    0.01 | 0.0458 |     800 B |        1.67 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,600.2 ns |  2.79 ns |  2.33 ns |  1.20 |    0.00 | 0.0553 |     952 B |        1.98 |
|                     |                |              |            |          |          |       |         |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,366.3 ns** | **11.85 ns** | **10.51 ns** |  **1.00** |    **0.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 5,302.0 ns | 20.64 ns | 18.30 ns |  1.21 |    0.00 | 0.0458 |     800 B |        0.17 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 4,624.1 ns | 36.43 ns | 32.29 ns |  1.06 |    0.01 | 0.2975 |    5048 B |        1.10 |
