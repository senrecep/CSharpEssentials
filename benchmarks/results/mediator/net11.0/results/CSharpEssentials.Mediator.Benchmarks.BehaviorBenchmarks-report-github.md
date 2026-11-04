```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **172.4 ns** | **0.82 ns** | **0.77 ns** |  **1.00** |    **0.01** | **0.0129** |     **216 B** |        **1.00** |
| Cse                 | Valid   | 205.8 ns | 0.78 ns | 0.73 ns |  1.19 |    0.01 | 0.0129 |     216 B |        1.00 |
| MediatRHandWritten  | Valid   | 288.2 ns | 2.58 ns | 2.41 ns |  1.67 |    0.02 | 0.0510 |     856 B |        3.96 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **236.6 ns** | **1.29 ns** | **1.20 ns** |  **1.00** |    **0.01** | **0.0424** |     **712 B** |        **1.00** |
| Cse                 | Invalid | 421.7 ns | 2.67 ns | 2.37 ns |  1.78 |    0.01 | 0.0615 |    1032 B |        1.45 |
| MediatRHandWritten  | Invalid | 362.7 ns | 2.59 ns | 2.42 ns |  1.53 |    0.01 | 0.0696 |    1168 B |        1.64 |
