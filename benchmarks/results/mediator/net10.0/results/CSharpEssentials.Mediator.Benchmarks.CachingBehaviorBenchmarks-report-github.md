```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Path | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |----- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Hit**  | **490.1 ns** | **1.71 ns** | **1.43 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| Cse                 | Hit  | 502.0 ns | 2.68 ns | 2.51 ns |  1.02 | 0.0181 |     304 B |        1.00 |
| MediatRHandWritten  | Hit  | 636.0 ns | 2.13 ns | 1.99 ns |  1.30 | 0.0391 |     664 B |        2.18 |
|                     |      |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Miss** | **572.5 ns** | **1.56 ns** | **1.38 ns** |  **1.00** | **0.0324** |     **552 B** |        **1.00** |
| Cse                 | Miss | 583.1 ns | 2.97 ns | 2.78 ns |  1.02 | 0.0324 |     552 B |        1.00 |
| MediatRHandWritten  | Miss | 863.3 ns | 5.97 ns | 5.29 ns |  1.51 | 0.0601 |    1008 B |        1.83 |
