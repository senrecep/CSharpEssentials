```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **194.2 ns** | **1.00 ns** | **0.94 ns** |  **1.00** | **0.0205** |     **344 B** |        **1.00** |
| Cse                 | Valid   | 296.5 ns | 1.33 ns | 1.25 ns |  1.53 | 0.0205 |     344 B |        1.00 |
| MediatRHandWritten  | Valid   | 436.0 ns | 2.29 ns | 2.03 ns |  2.24 | 0.0639 |    1072 B |        3.12 |
|                     |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Invalid** | **417.4 ns** | **2.42 ns** | **2.26 ns** |  **1.00** | **0.0582** |     **976 B** |        **1.00** |
| Cse                 | Invalid | 729.4 ns | 3.35 ns | 3.13 ns |  1.75 | 0.0744 |    1256 B |        1.29 |
| MediatRHandWritten  | Invalid | 661.5 ns | 3.76 ns | 3.52 ns |  1.58 | 0.0906 |    1520 B |        1.56 |
