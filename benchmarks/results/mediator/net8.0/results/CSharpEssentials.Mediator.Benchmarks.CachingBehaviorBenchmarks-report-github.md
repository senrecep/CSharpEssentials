```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Path | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |----- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Hit**  | **560.2 ns** | **0.66 ns** | **0.62 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| Cse                 | Hit  | 613.0 ns | 1.19 ns | 1.05 ns |  1.09 | 0.0181 |     304 B |        1.00 |
| MediatRHandWritten  | Hit  | 783.9 ns | 6.99 ns | 6.54 ns |  1.40 | 0.0429 |     728 B |        2.39 |
|                     |      |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Miss** | **675.1 ns** | **1.53 ns** | **1.43 ns** |  **1.00** | **0.0324** |     **552 B** |        **1.00** |
| Cse                 | Miss | 734.7 ns | 1.02 ns | 0.80 ns |  1.09 | 0.0324 |     552 B |        1.00 |
| MediatRHandWritten  | Miss | 945.6 ns | 2.93 ns | 2.74 ns |  1.40 | 0.0639 |    1072 B |        1.94 |
