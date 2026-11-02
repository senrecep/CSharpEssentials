```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **201.9 ns** | **0.99 ns** | **0.88 ns** |  **1.00** |    **0.01** | **0.0205** |     **344 B** |        **1.00** |
| Cse                 | Valid   | 234.3 ns | 1.18 ns | 1.10 ns |  1.16 |    0.01 | 0.0205 |     344 B |        1.00 |
| MediatRHandWritten  | Valid   | 410.6 ns | 2.23 ns | 2.09 ns |  2.03 |    0.01 | 0.0639 |    1072 B |        3.12 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **336.1 ns** | **6.72 ns** | **8.25 ns** |  **1.00** |    **0.03** | **0.0525** |     **880 B** |        **1.00** |
| Cse                 | Invalid | 620.0 ns | 5.66 ns | 4.72 ns |  1.85 |    0.05 | 0.0687 |    1160 B |        1.32 |
| MediatRHandWritten  | Invalid | 548.4 ns | 2.67 ns | 2.49 ns |  1.63 |    0.04 | 0.0849 |    1424 B |        1.62 |
