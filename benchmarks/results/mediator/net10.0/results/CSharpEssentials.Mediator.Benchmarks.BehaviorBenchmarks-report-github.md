```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **183.5 ns** | **0.63 ns** | **0.59 ns** |  **1.00** |    **0.00** | **0.0129** |     **216 B** |        **1.00** |
| Cse                 | Valid   | 236.2 ns | 0.90 ns | 0.84 ns |  1.29 |    0.01 | 0.0129 |     216 B |        1.00 |
| MediatRHandWritten  | Valid   | 370.7 ns | 2.04 ns | 1.90 ns |  2.02 |    0.01 | 0.0525 |     880 B |        4.07 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **262.9 ns** | **2.76 ns** | **2.58 ns** |  **1.00** |    **0.01** | **0.0424** |     **712 B** |        **1.00** |
| Cse                 | Invalid | 439.8 ns | 2.73 ns | 2.55 ns |  1.67 |    0.02 | 0.0615 |    1032 B |        1.45 |
| MediatRHandWritten  | Invalid | 428.7 ns | 3.51 ns | 3.28 ns |  1.63 |    0.02 | 0.0710 |    1192 B |        1.67 |
