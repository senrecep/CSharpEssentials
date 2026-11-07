```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method              | Path           | Fingerprint  | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------- |------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Replay**         | **Supplied**     |   **526.2 ns** |  **2.55 ns** |  **2.39 ns** |  **1.00** | **0.0372** |     **632 B** |        **1.00** |
| Cse                 | Replay         | Supplied     |   789.8 ns |  1.81 ns |  1.60 ns |  1.50 | 0.0401 |     672 B |        1.06 |
| MediatRHandWritten  | Replay         | Supplied     |   642.9 ns |  3.83 ns |  3.58 ns |  1.22 | 0.0582 |     984 B |        1.56 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **Default**      | **1,654.3 ns** |  **4.00 ns** |  **3.75 ns** |  **1.00** | **0.0572** |     **968 B** |        **1.00** |
| Cse                 | Replay         | Default      | 1,828.3 ns |  7.94 ns |  7.43 ns |  1.11 | 0.0477 |     824 B |        0.85 |
| MediatRHandWritten  | Replay         | Default      | 1,840.3 ns |  5.10 ns |  4.77 ns |  1.11 | 0.0782 |    1320 B |        1.36 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **Replay**         | **DefaultLarge** | **4,692.1 ns** | **23.39 ns** | **21.88 ns** |  **1.00** | **0.2975** |    **5064 B** |        **1.00** |
| Cse                 | Replay         | DefaultLarge | 4,487.2 ns |  7.68 ns |  6.81 ns |  0.96 | 0.0458 |     824 B |        0.16 |
| MediatRHandWritten  | Replay         | DefaultLarge | 4,894.4 ns | 19.75 ns | 17.51 ns |  1.04 | 0.3204 |    5416 B |        1.07 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Supplied**     |   **292.6 ns** |  **0.45 ns** |  **0.42 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| Cse                 | ExecuteRelease | Supplied     |   644.0 ns |  1.59 ns |  1.48 ns |  2.20 | 0.0191 |     328 B |        2.28 |
| MediatRHandWritten  | ExecuteRelease | Supplied     |   434.8 ns |  1.27 ns |  1.19 ns |  1.49 | 0.0358 |     600 B |        4.17 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **Default**      | **1,278.1 ns** |  **2.52 ns** |  **2.36 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| Cse                 | ExecuteRelease | Default      | 1,501.4 ns |  3.25 ns |  3.04 ns |  1.17 | 0.0286 |     480 B |        1.00 |
| MediatRHandWritten  | ExecuteRelease | Default      | 1,498.1 ns |  4.00 ns |  3.74 ns |  1.17 | 0.0553 |     936 B |        1.95 |
|                     |                |              |            |          |          |       |        |           |             |
| **MediatorHandWritten** | **ExecuteRelease** | **DefaultLarge** | **4,184.5 ns** | **19.40 ns** | **18.15 ns** |  **1.00** | **0.2670** |    **4576 B** |        **1.00** |
| Cse                 | ExecuteRelease | DefaultLarge | 4,279.6 ns |  6.56 ns |  5.81 ns |  1.02 | 0.0229 |     480 B |        0.10 |
| MediatRHandWritten  | ExecuteRelease | DefaultLarge | 4,493.4 ns | 17.03 ns | 15.09 ns |  1.07 | 0.2975 |    5032 B |        1.10 |
