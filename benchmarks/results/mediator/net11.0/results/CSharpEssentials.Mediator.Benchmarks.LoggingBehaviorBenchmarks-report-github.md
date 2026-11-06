```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Logger   | Payload         | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------- |---------------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Disabled** | **None**            |   **100.20 ns** | **0.160 ns** | **0.142 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | None            |    99.69 ns | 0.025 ns | 0.024 ns |  0.99 |      - |         - |          NA |
| MediatRHandWritten  | Disabled | None            |   216.56 ns | 1.071 ns | 1.002 ns |  2.16 | 0.0272 |     456 B |          NA |
|                     |          |                 |             |          |          |       |        |           |             |
| **MediatorHandWritten** | **Disabled** | **RequestResponse** |   **101.78 ns** | **0.058 ns** | **0.054 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Cse                 | Disabled | RequestResponse |    99.96 ns | 0.048 ns | 0.045 ns |  0.98 |      - |         - |          NA |
| MediatRHandWritten  | Disabled | RequestResponse |   215.37 ns | 1.065 ns | 0.944 ns |  2.12 | 0.0272 |     456 B |          NA |
|                     |          |                 |             |          |          |       |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **None**            |   **291.21 ns** | **0.687 ns** | **0.643 ns** |  **1.00** | **0.0143** |     **240 B** |        **1.00** |
| Cse                 | Enabled  | None            |   287.53 ns | 0.435 ns | 0.386 ns |  0.99 | 0.0143 |     240 B |        1.00 |
| MediatRHandWritten  | Enabled  | None            |   449.77 ns | 1.428 ns | 1.266 ns |  1.54 | 0.0415 |     696 B |        2.90 |
|                     |          |                 |             |          |          |       |        |           |             |
| **MediatorHandWritten** | **Enabled**  | **RequestResponse** | **1,088.65 ns** | **1.728 ns** | **1.532 ns** |  **1.00** | **0.0725** |    **1232 B** |        **1.00** |
| Cse                 | Enabled  | RequestResponse | 1,037.73 ns | 5.853 ns | 5.189 ns |  0.95 | 0.0725 |    1232 B |        1.00 |
| MediatRHandWritten  | Enabled  | RequestResponse | 1,219.56 ns | 3.685 ns | 3.447 ns |  1.12 | 0.0992 |    1688 B |        1.37 |
