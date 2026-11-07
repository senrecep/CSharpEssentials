```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **592.3 ns** |  **1.81 ns** |  **1.51 ns** |  **1.00** | **0.0391** |     **664 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   906.6 ns |  6.57 ns |  6.14 ns |  1.53 | 0.0420 |     704 B |        1.06 |
| MediatRHandWritten  | Replay         | Supplied     |   790.7 ns |  3.07 ns |  2.87 ns |  1.34 | 0.0648 |    1096 B |        1.65 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **2,122.7 ns** |  **5.38 ns** |  **4.77 ns** |  **1.00** | **0.0572** |    **1000 B** |        **1.00** |
| Cse                 | Replay         | Default      | 3,151.6 ns |  8.46 ns |  7.50 ns |  1.48 | 0.0801 |    1384 B |        1.38 |
| MediatRHandWritten  | Replay         | Default      | 2,377.9 ns | 16.39 ns | 14.53 ns |  1.12 | 0.0839 |    1432 B |        1.43 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **5,141.2 ns** | **18.01 ns** | **15.97 ns** |  **1.00** | **0.2975** |    **5096 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 6,182.3 ns | 22.31 ns | 20.87 ns |  1.20 | 0.3204 |    5480 B |        1.08 |
| MediatRHandWritten  | Replay         | DefaultLarge | 5,460.1 ns | 20.34 ns | 19.02 ns |  1.06 | 0.3281 |    5528 B |        1.08 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **310.3 ns** |  **0.27 ns** |  **0.22 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     |   735.5 ns |  0.89 ns |  0.83 ns |  2.37 | 0.0191 |     328 B |        2.28 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   542.8 ns |  1.21 ns |  1.08 ns |  1.75 | 0.0401 |     680 B |        4.72 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,598.3 ns** |  **2.41 ns** |  **1.88 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 2,792.1 ns |  6.72 ns |  5.96 ns |  1.75 | 0.0572 |    1008 B |        2.10 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,982.5 ns |  6.47 ns |  6.05 ns |  1.24 | 0.0572 |    1016 B |        2.12 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,771.4 ns** | **18.73 ns** | **17.52 ns** |  **1.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 5,839.4 ns | 21.68 ns | 20.28 ns |  1.22 | 0.2975 |    5104 B |        1.12 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 4,972.3 ns | 19.27 ns | 18.03 ns |  1.04 | 0.3052 |    5112 B |        1.12 |
