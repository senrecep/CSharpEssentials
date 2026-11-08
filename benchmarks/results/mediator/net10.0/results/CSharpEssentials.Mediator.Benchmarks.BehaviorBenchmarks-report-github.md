```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **179.3 ns** | **0.71 ns** | **0.67 ns** |  **1.00** |    **0.01** | **0.0129** |     **216 B** |        **1.00** |
| Cse                 | Valid   | 177.6 ns | 1.08 ns | 1.01 ns |  0.99 |    0.01 | 0.0129 |     216 B |        1.00 |
| MediatRHandWritten  | Valid   | 402.4 ns | 3.85 ns | 3.42 ns |  2.24 |    0.02 | 0.0525 |     880 B |        4.07 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **282.1 ns** | **2.22 ns** | **1.96 ns** |  **1.00** |    **0.01** | **0.0424** |     **712 B** |        **1.00** |
| Cse                 | Invalid | 292.5 ns | 5.68 ns | 6.76 ns |  1.04 |    0.02 | 0.0448 |     752 B |        1.06 |
| MediatRHandWritten  | Invalid | 396.0 ns | 3.14 ns | 2.94 ns |  1.40 |    0.01 | 0.0710 |    1192 B |        1.67 |
