```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **213.7 ns** | **1.01 ns** | **0.89 ns** |  **1.00** | **0.0205** |     **344 B** |        **1.00** |
| Cse                 | Valid   | 208.8 ns | 0.98 ns | 0.92 ns |  0.98 | 0.0205 |     344 B |        1.00 |
| MediatRHandWritten  | Valid   | 489.4 ns | 2.19 ns | 2.05 ns |  2.29 | 0.0639 |    1072 B |        3.12 |
|                     |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Invalid** | **435.6 ns** | **1.92 ns** | **1.80 ns** |  **1.00** | **0.0582** |     **976 B** |        **1.00** |
| Cse                 | Invalid | 490.5 ns | 4.11 ns | 3.85 ns |  1.13 | 0.0582 |     976 B |        1.00 |
| MediatRHandWritten  | Invalid | 620.4 ns | 4.07 ns | 3.81 ns |  1.42 | 0.0906 |    1520 B |        1.56 |
