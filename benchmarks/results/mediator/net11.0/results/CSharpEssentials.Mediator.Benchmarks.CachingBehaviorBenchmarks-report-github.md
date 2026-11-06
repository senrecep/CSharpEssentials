```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Path | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |----- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Hit**  | **462.2 ns** | **0.82 ns** | **0.77 ns** |  **1.00** | **0.0181** |     **304 B** |        **1.00** |
| Cse                 | Hit  | 484.3 ns | 1.23 ns | 1.09 ns |  1.05 | 0.0181 |     304 B |        1.00 |
| MediatRHandWritten  | Hit  | 596.4 ns | 4.52 ns | 3.78 ns |  1.29 | 0.0381 |     648 B |        2.13 |
|                     |      |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Miss** | **540.8 ns** | **1.14 ns** | **1.07 ns** |  **1.00** | **0.0277** |     **472 B** |        **1.00** |
| Cse                 | Miss | 550.2 ns | 1.14 ns | 1.01 ns |  1.02 | 0.0277 |     472 B |        1.00 |
| MediatRHandWritten  | Miss | 771.3 ns | 1.16 ns | 1.09 ns |  1.43 | 0.0544 |     912 B |        1.93 |
