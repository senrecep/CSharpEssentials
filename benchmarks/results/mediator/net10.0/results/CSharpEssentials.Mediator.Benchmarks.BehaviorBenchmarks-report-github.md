```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **198.0 ns** | **0.55 ns** | **0.52 ns** |  **1.00** | **0.0129** |     **216 B** |        **1.00** |
| Cse                 | Valid   | 163.7 ns | 0.59 ns | 0.55 ns |  0.83 | 0.0129 |     216 B |        1.00 |
| MediatRHandWritten  | Valid   | 329.6 ns | 1.86 ns | 1.74 ns |  1.66 | 0.0525 |     880 B |        4.07 |
|                     |         |          |         |         |       |        |           |             |
| **MediatorHandWritten** | **Invalid** | **273.6 ns** | **1.62 ns** | **1.52 ns** |  **1.00** | **0.0424** |     **712 B** |        **1.00** |
| Cse                 | Invalid | 303.9 ns | 1.31 ns | 1.16 ns |  1.11 | 0.0448 |     752 B |        1.06 |
| MediatRHandWritten  | Invalid | 403.3 ns | 3.36 ns | 2.98 ns |  1.47 | 0.0710 |    1192 B |        1.67 |
