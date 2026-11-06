```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **610.5 ns** |  **1.97 ns** |  **1.85 ns** |  **1.00** | **0.0391** |     **664 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   984.9 ns |  3.26 ns |  2.89 ns |  1.61 | 0.0610 |    1032 B |        1.55 |
| MediatRHandWritten  | Replay         | Supplied     |   811.6 ns |  2.48 ns |  2.32 ns |  1.33 | 0.0648 |    1096 B |        1.65 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **2,159.4 ns** |  **3.89 ns** |  **3.64 ns** |  **1.00** | **0.0572** |    **1000 B** |        **1.00** |
| Cse                 | Replay         | Default      | 3,276.1 ns |  5.82 ns |  5.45 ns |  1.52 | 0.0992 |    1712 B |        1.71 |
| MediatRHandWritten  | Replay         | Default      | 2,382.6 ns |  4.73 ns |  4.42 ns |  1.10 | 0.0839 |    1432 B |        1.43 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **5,249.6 ns** | **16.06 ns** | **15.02 ns** |  **1.00** | **0.2975** |    **5096 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 6,422.5 ns | 36.41 ns | 34.06 ns |  1.22 | 0.3433 |    5808 B |        1.14 |
| MediatRHandWritten  | Replay         | DefaultLarge | 5,476.3 ns | 18.44 ns | 17.25 ns |  1.04 | 0.3281 |    5528 B |        1.08 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **314.4 ns** |  **0.34 ns** |  **0.30 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     | 1,894.3 ns |  2.83 ns |  2.51 ns |  6.02 | 0.0381 |     648 B |        4.50 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   536.2 ns |  1.63 ns |  1.53 ns |  1.71 | 0.0401 |     680 B |        4.72 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,641.3 ns** |  **3.43 ns** |  **3.04 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 4,016.9 ns |  5.99 ns |  5.60 ns |  2.45 | 0.0763 |    1328 B |        2.77 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,960.2 ns |  3.07 ns |  2.87 ns |  1.19 | 0.0572 |    1016 B |        2.12 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,881.7 ns** | **19.11 ns** | **17.87 ns** |  **1.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 7,180.7 ns | 17.12 ns | 16.01 ns |  1.47 | 0.3204 |    5424 B |        1.19 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 5,109.3 ns | 15.42 ns | 14.43 ns |  1.05 | 0.3052 |    5112 B |        1.12 |
