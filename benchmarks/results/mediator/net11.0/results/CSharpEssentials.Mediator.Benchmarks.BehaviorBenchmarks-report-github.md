```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **172.0 ns** | **0.54 ns** | **0.50 ns** |  **1.00** | **0.0129** |     **216 B** |        **1.00** |
| Cse                 | Valid   | 170.2 ns | 0.47 ns | 0.44 ns |  0.99 | 0.0129 |     216 B |        1.00 |
| MediatRHandWritten  | Valid   | 321.2 ns | 0.88 ns | 0.74 ns |  1.87 | 0.0510 |     856 B |        3.96 |
|                     |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Invalid** | **249.0 ns** | **1.78 ns** | **1.66 ns** |  **1.00** | **0.0424** |     **712 B** |        **1.00** |
| Cse                 | Invalid | 272.0 ns | 0.98 ns | 0.87 ns |  1.09 | 0.0448 |     752 B |        1.06 |
| MediatRHandWritten  | Invalid | 382.4 ns | 3.05 ns | 2.85 ns |  1.54 | 0.0696 |    1168 B |        1.64 |
