```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Logger   | Payload         | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------- |---------------- |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Disabled** | **None**            |   **106.2 ns** | **0.04 ns** | **0.03 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | None            |   109.3 ns | 0.23 ns | 0.22 ns |  1.03 |    0.00 |      - |         - |          NA |
| MediatRHandWritten  | Disabled | None            |   319.4 ns | 1.69 ns | 1.58 ns |  3.01 |    0.01 | 0.0319 |     536 B |          NA |
|                     |          |                 |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Disabled** | **RequestResponse** |   **101.9 ns** | **0.08 ns** | **0.07 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | RequestResponse |   105.1 ns | 0.03 ns | 0.03 ns |  1.03 |    0.00 |      - |         - |          NA |
| MediatRHandWritten  | Disabled | RequestResponse |   275.3 ns | 0.85 ns | 0.75 ns |  2.70 |    0.01 | 0.0319 |     536 B |          NA |
|                     |          |                 |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **None**            |   **320.4 ns** | **0.66 ns** | **0.62 ns** |  **1.00** |    **0.00** | **0.0143** |     **240 B** |        **1.00** |
| Cse                 | Enabled  | None            |   320.6 ns | 2.59 ns | 2.42 ns |  1.00 |    0.01 | 0.0143 |     240 B |        1.00 |
| MediatRHandWritten  | Enabled  | None            |   536.1 ns | 5.64 ns | 5.28 ns |  1.67 |    0.02 | 0.0458 |     776 B |        3.23 |
|                     |          |                 |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **RequestResponse** | **1,279.2 ns** | **3.77 ns** | **3.15 ns** |  **1.00** |    **0.00** | **0.0725** |    **1232 B** |        **1.00** |
| Cse                 | Enabled  | RequestResponse | 1,219.9 ns | 2.61 ns | 2.45 ns |  0.95 |    0.00 | 0.0725 |    1232 B |        1.00 |
| MediatRHandWritten  | Enabled  | RequestResponse | 1,531.5 ns | 5.30 ns | 4.96 ns |  1.20 |    0.00 | 0.1049 |    1768 B |        1.44 |
