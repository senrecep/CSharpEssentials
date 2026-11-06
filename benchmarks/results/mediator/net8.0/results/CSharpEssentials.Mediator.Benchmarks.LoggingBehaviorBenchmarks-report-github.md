```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Logger   | Payload         | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------- |---------------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Disabled** | **None**            |   **139.0 ns** | **0.17 ns** | **0.16 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | None            |   133.1 ns | 0.11 ns | 0.10 ns |  0.96 | 0.0024 |      40 B |          NA |
| MediatRHandWritten  | Disabled | None            |   374.0 ns | 1.46 ns | 1.29 ns |  2.69 | 0.0319 |     536 B |          NA |
|                     |          |                 |            |         |         |       |        |           |             |
| **MediatorHandWritten** | **Disabled** | **RequestResponse** |   **111.3 ns** | **0.07 ns** | **0.06 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | RequestResponse |   130.7 ns | 0.12 ns | 0.11 ns |  1.17 | 0.0024 |      40 B |          NA |
| MediatRHandWritten  | Disabled | RequestResponse |   312.0 ns | 1.16 ns | 1.08 ns |  2.80 | 0.0319 |     536 B |          NA |
|                     |          |                 |            |         |         |       |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **None**            |   **414.3 ns** | **0.58 ns** | **0.51 ns** |  **1.00** | **0.0143** |     **240 B** |        **1.00** |
| Cse                 | Enabled  | None            |   417.5 ns | 2.25 ns | 2.11 ns |  1.01 | 0.0167 |     280 B |        1.17 |
| MediatRHandWritten  | Enabled  | None            |   688.6 ns | 1.68 ns | 1.49 ns |  1.66 | 0.0458 |     776 B |        3.23 |
|                     |          |                 |            |         |         |       |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **RequestResponse** | **1,414.1 ns** | **4.77 ns** | **4.23 ns** |  **1.00** | **0.0725** |    **1232 B** |        **1.00** |
| Cse                 | Enabled  | RequestResponse | 1,426.1 ns | 4.20 ns | 3.93 ns |  1.01 | 0.0744 |    1272 B |        1.03 |
| MediatRHandWritten  | Enabled  | RequestResponse | 1,827.3 ns | 3.73 ns | 3.49 ns |  1.29 | 0.1049 |    1768 B |        1.44 |
