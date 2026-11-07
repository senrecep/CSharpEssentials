```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **551.1 ns** |  **2.43 ns** |  **2.28 ns** |  **1.00** | **0.0372** |     **632 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   827.0 ns |  3.07 ns |  2.40 ns |  1.50 | 0.0401 |     672 B |        1.06 |
| MediatRHandWritten  | Replay         | Supplied     |   750.5 ns |  3.92 ns |  3.67 ns |  1.36 | 0.0629 |    1064 B |        1.68 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **2,115.6 ns** |  **5.72 ns** |  **5.35 ns** |  **1.00** | **0.0572** |     **968 B** |        **1.00** |
| Cse                 | Replay         | Default      | 2,305.9 ns |  5.20 ns |  4.61 ns |  1.09 | 0.0572 |    1000 B |        1.03 |
| MediatRHandWritten  | Replay         | Default      | 2,291.5 ns |  7.24 ns |  6.78 ns |  1.08 | 0.0801 |    1400 B |        1.45 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **5,099.5 ns** | **19.81 ns** | **18.53 ns** |  **1.00** | **0.2975** |    **5064 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 5,028.9 ns | 10.24 ns |  9.58 ns |  0.99 | 0.0534 |    1000 B |        0.20 |
| MediatRHandWritten  | Replay         | DefaultLarge | 5,324.5 ns | 26.19 ns | 24.49 ns |  1.04 | 0.3281 |    5496 B |        1.09 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **297.6 ns** |  **0.93 ns** |  **0.87 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     |   669.5 ns |  1.70 ns |  1.59 ns |  2.25 | 0.0191 |     328 B |        2.28 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   485.3 ns |  2.71 ns |  2.40 ns |  1.63 | 0.0401 |     680 B |        4.72 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,569.1 ns** |  **3.32 ns** |  **2.94 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 1,985.1 ns |  4.59 ns |  4.07 ns |  1.27 | 0.0381 |     656 B |        1.37 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,923.1 ns |  6.20 ns |  5.80 ns |  1.23 | 0.0572 |    1016 B |        2.12 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,662.4 ns** | **14.58 ns** | **12.92 ns** |  **1.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 4,796.3 ns |  9.98 ns |  8.85 ns |  1.03 | 0.0381 |     656 B |        0.14 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 4,968.5 ns | 26.51 ns | 24.80 ns |  1.07 | 0.3052 |    5112 B |        1.12 |
