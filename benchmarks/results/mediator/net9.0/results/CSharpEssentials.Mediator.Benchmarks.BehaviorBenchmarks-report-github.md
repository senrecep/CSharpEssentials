```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **223.4 ns** | **1.20 ns** | **1.12 ns** |  **1.00** |    **0.01** | **0.0205** |     **344 B** |        **1.00** |
| Cse                 | Valid   | 205.0 ns | 0.63 ns | 0.59 ns |  0.92 |    0.01 | 0.0205 |     344 B |        1.00 |
| MediatRHandWritten  | Valid   | 498.6 ns | 5.27 ns | 4.67 ns |  2.23 |    0.02 | 0.0639 |    1072 B |        3.12 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **334.9 ns** | **1.54 ns** | **1.44 ns** |  **1.00** |    **0.01** | **0.0525** |     **880 B** |        **1.00** |
| Cse                 | Invalid | 379.9 ns | 2.19 ns | 2.05 ns |  1.13 |    0.01 | 0.0553 |     928 B |        1.05 |
| MediatRHandWritten  | Invalid | 586.5 ns | 2.42 ns | 2.26 ns |  1.75 |    0.01 | 0.0849 |    1424 B |        1.62 |
