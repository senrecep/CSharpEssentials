```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **532.2 ns** |  **1.75 ns** |  **1.64 ns** |  **1.00** | **0.0372** |     **632 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   876.3 ns |  5.88 ns |  5.22 ns |  1.65 | 0.0591 |     992 B |        1.57 |
| MediatRHandWritten  | Replay         | Supplied     |   640.2 ns |  2.23 ns |  2.08 ns |  1.20 | 0.0582 |     984 B |        1.56 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **1,714.0 ns** |  **9.25 ns** |  **8.65 ns** |  **1.00** | **0.0572** |     **968 B** |        **1.00** |
| Cse                 | Replay         | Default      | 1,895.9 ns |  3.62 ns |  3.39 ns |  1.11 | 0.0668 |    1144 B |        1.18 |
| MediatRHandWritten  | Replay         | Default      | 1,876.4 ns | 14.77 ns | 13.81 ns |  1.09 | 0.0782 |    1320 B |        1.36 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **4,740.7 ns** | **28.98 ns** | **27.11 ns** |  **1.00** | **0.2899** |    **5064 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 4,606.4 ns |  5.73 ns |  4.78 ns |  0.97 | 0.0610 |    1144 B |        0.23 |
| MediatRHandWritten  | Replay         | DefaultLarge | 4,918.5 ns | 13.53 ns | 11.30 ns |  1.04 | 0.3204 |    5416 B |        1.07 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **296.1 ns** |  **0.87 ns** |  **0.81 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     |   763.5 ns |  1.78 ns |  1.67 ns |  2.58 | 0.0381 |     640 B |        4.44 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   426.8 ns |  1.50 ns |  1.33 ns |  1.44 | 0.0358 |     600 B |        4.17 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,260.7 ns** |  **1.15 ns** |  **0.96 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 1,645.1 ns |  2.81 ns |  2.63 ns |  1.30 | 0.0458 |     792 B |        1.65 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,488.1 ns |  8.40 ns |  7.02 ns |  1.18 | 0.0553 |     936 B |        1.95 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,303.3 ns** | **22.61 ns** | **21.15 ns** |  **1.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 4,386.9 ns | 17.54 ns | 16.40 ns |  1.02 | 0.0458 |     792 B |        0.17 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 4,517.8 ns | 23.99 ns | 22.44 ns |  1.05 | 0.2975 |    5032 B |        1.10 |
