```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Input   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| **MediatorHandWritten** | **Valid**   | **195.9 ns** | **0.52 ns** | **0.49 ns** |  **1.00** |    **0.00** | **0.0205** |     **344 B** |        **1.00** |
| Cse                 | Valid   | 199.9 ns | 0.88 ns | 0.82 ns |  1.02 |    0.00 | 0.0205 |     344 B |        1.00 |
| MediatRHandWritten  | Valid   | 516.8 ns | 1.17 ns | 0.97 ns |  2.64 |    0.01 | 0.0639 |    1072 B |        3.12 |
|                     |         |          |         |         |       |         |        |           |             |
| **MediatorHandWritten** | **Invalid** | **443.1 ns** | **5.53 ns** | **5.17 ns** |  **1.00** |    **0.02** | **0.0582** |     **976 B** |        **1.00** |
| Cse                 | Invalid | 493.2 ns | 2.74 ns | 2.56 ns |  1.11 |    0.01 | 0.0582 |     976 B |        1.00 |
| MediatRHandWritten  | Invalid | 638.0 ns | 5.98 ns | 5.59 ns |  1.44 |    0.02 | 0.0906 |    1520 B |        1.56 |
