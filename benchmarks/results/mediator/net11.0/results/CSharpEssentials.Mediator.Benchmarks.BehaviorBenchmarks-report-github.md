```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **168.5 ns** | **0.88 ns** | **0.78 ns** |  **1.00** |    **0.01** | **0.0129** |     **216 B** |        **1.00** |
| Cse                 | Valid   | 172.6 ns | 0.82 ns | 0.77 ns |  1.02 |    0.01 | 0.0129 |     216 B |        1.00 |
| MediatRHandWritten  | Valid   | 314.2 ns | 5.07 ns | 4.49 ns |  1.86 |    0.03 | 0.0510 |     856 B |        3.96 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **255.4 ns** | **3.44 ns** | **2.69 ns** |  **1.00** |    **0.01** | **0.0424** |     **712 B** |        **1.00** |
| Cse                 | Invalid | 260.6 ns | 1.98 ns | 1.85 ns |  1.02 |    0.01 | 0.0448 |     752 B |        1.06 |
| MediatRHandWritten  | Invalid | 367.1 ns | 2.89 ns | 2.70 ns |  1.44 |    0.02 | 0.0696 |    1168 B |        1.64 |
