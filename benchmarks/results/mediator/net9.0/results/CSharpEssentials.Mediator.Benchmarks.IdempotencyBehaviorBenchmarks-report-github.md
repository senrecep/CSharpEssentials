```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **563.9 ns** |  **3.66 ns** |  **3.25 ns** |  **1.00** | **0.0372** |     **632 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   890.6 ns |  7.01 ns |  6.21 ns |  1.58 | 0.0591 |    1000 B |        1.58 |
| MediatRHandWritten  | Replay         | Supplied     |   755.6 ns |  3.61 ns |  3.01 ns |  1.34 | 0.0629 |    1064 B |        1.68 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **2,090.3 ns** |  **3.90 ns** |  **3.65 ns** |  **1.00** | **0.0572** |     **968 B** |        **1.00** |
| Cse                 | Replay         | Default      | 2,362.9 ns |  4.22 ns |  3.52 ns |  1.13 | 0.0763 |    1328 B |        1.37 |
| MediatRHandWritten  | Replay         | Default      | 2,356.6 ns |  6.59 ns |  6.16 ns |  1.13 | 0.0801 |    1400 B |        1.45 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **5,209.9 ns** | **23.65 ns** | **20.97 ns** |  **1.00** | **0.2975** |    **5064 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 5,215.2 ns | 11.17 ns |  9.90 ns |  1.00 | 0.0763 |    1328 B |        0.26 |
| MediatRHandWritten  | Replay         | DefaultLarge | 5,488.1 ns | 15.94 ns | 14.13 ns |  1.05 | 0.3281 |    5496 B |        1.09 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **299.2 ns** |  **0.49 ns** |  **0.46 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     | 1,701.9 ns |  2.67 ns |  2.37 ns |  5.69 | 0.0381 |     648 B |        4.50 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   489.5 ns |  1.06 ns |  0.99 ns |  1.64 | 0.0401 |     680 B |        4.72 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,583.5 ns** |  **6.13 ns** |  **5.73 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 3,154.2 ns |  2.05 ns |  1.82 ns |  1.99 | 0.0572 |     976 B |        2.03 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,851.4 ns |  3.28 ns |  2.90 ns |  1.17 | 0.0591 |    1016 B |        2.12 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,776.4 ns** | **16.63 ns** | **15.56 ns** |  **1.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 5,939.2 ns | 12.32 ns | 10.93 ns |  1.24 | 0.0534 |     976 B |        0.21 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 5,038.2 ns | 13.27 ns | 12.42 ns |  1.05 | 0.3052 |    5112 B |        1.12 |
