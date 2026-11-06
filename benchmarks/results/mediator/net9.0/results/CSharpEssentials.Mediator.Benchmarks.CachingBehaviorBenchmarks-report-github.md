```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Path | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |----- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Hit**  | **519.9 ns** | **1.54 ns** | **1.37 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| Cse                 | Hit  | 539.5 ns | 1.30 ns | 1.16 ns |  1.04 | 0.0181 |     304 B |        1.00 |
| MediatRHandWritten  | Hit  | 718.3 ns | 1.84 ns | 1.72 ns |  1.38 | 0.0429 |     728 B |        2.39 |
|                     |      |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Miss** | **631.0 ns** | **1.74 ns** | **1.45 ns** |  **1.00** | **0.0324** |     **552 B** |        **1.00** |
| Cse                 | Miss | 660.4 ns | 1.27 ns | 1.19 ns |  1.05 | 0.0324 |     552 B |        1.00 |
| MediatRHandWritten  | Miss | 955.8 ns | 8.14 ns | 7.61 ns |  1.51 | 0.0639 |    1072 B |        1.94 |
