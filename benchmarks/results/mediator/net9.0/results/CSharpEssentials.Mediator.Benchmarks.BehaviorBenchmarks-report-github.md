```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **205.0 ns** | **2.40 ns** | **2.12 ns** |  **1.00** |    **0.01** | **0.0205** |     **344 B** |        **1.00** |
| Cse                 | Valid   | 199.4 ns | 1.02 ns | 0.91 ns |  0.97 |    0.01 | 0.0205 |     344 B |        1.00 |
| MediatRHandWritten  | Valid   | 431.9 ns | 3.86 ns | 3.61 ns |  2.11 |    0.03 | 0.0639 |    1072 B |        3.12 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **327.4 ns** | **2.69 ns** | **2.52 ns** |  **1.00** |    **0.01** | **0.0525** |     **880 B** |        **1.00** |
| Cse                 | Invalid | 364.3 ns | 2.80 ns | 2.62 ns |  1.11 |    0.01 | 0.0553 |     928 B |        1.05 |
| MediatRHandWritten  | Invalid | 498.8 ns | 2.95 ns | 2.62 ns |  1.52 |    0.01 | 0.0849 |    1424 B |        1.62 |
