```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Logger   | Payload         | Mean       | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------- |---------------- |-----------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Disabled** | **None**            |   **103.7 ns** | **0.08 ns** | **0.07 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | None            |   109.3 ns | 0.62 ns | 0.58 ns |  1.05 |    0.01 |      - |         - |          NA |
| MediatRHandWritten  | Disabled | None            |   229.8 ns | 1.39 ns | 1.30 ns |  2.22 |    0.01 | 0.0281 |     472 B |          NA |
|                     |          |                 |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Disabled** | **RequestResponse** |   **102.7 ns** | **0.05 ns** | **0.04 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | RequestResponse |   102.5 ns | 0.03 ns | 0.02 ns |  1.00 |    0.00 |      - |         - |          NA |
| MediatRHandWritten  | Disabled | RequestResponse |   226.5 ns | 3.11 ns | 2.90 ns |  2.20 |    0.03 | 0.0281 |     472 B |          NA |
|                     |          |                 |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **None**            |   **293.4 ns** | **0.66 ns** | **0.62 ns** |  **1.00** |    **0.00** | **0.0143** |     **240 B** |        **1.00** |
| Cse                 | Enabled  | None            |   291.1 ns | 0.51 ns | 0.43 ns |  0.99 |    0.00 | 0.0143 |     240 B |        1.00 |
| MediatRHandWritten  | Enabled  | None            |   471.2 ns | 1.61 ns | 1.50 ns |  1.61 |    0.01 | 0.0424 |     712 B |        2.97 |
|                     |          |                 |            |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **RequestResponse** | **1,039.8 ns** | **3.64 ns** | **3.23 ns** |  **1.00** |    **0.00** | **0.0725** |    **1232 B** |        **1.00** |
| Cse                 | Enabled  | RequestResponse | 1,042.5 ns | 3.55 ns | 3.32 ns |  1.00 |    0.00 | 0.0725 |    1232 B |        1.00 |
| MediatRHandWritten  | Enabled  | RequestResponse | 1,308.2 ns | 3.50 ns | 2.92 ns |  1.26 |    0.00 | 0.1011 |    1704 B |        1.38 |
