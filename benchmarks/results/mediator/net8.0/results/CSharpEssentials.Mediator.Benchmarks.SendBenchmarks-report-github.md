```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         | **155.268 ns** | **0.6794 ns** | **0.6355 ns** |  **1.00** | **0.0186** |     **312 B** |        **1.00** |
| MediatorIMediator | 0         |  11.257 ns | 0.0096 ns | 0.0090 ns |  0.07 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.951 ns | 0.0108 ns | 0.0101 ns |  0.02 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **238.648 ns** | **0.7706 ns** | **0.6016 ns** |  **1.00** | **0.0386** |     **648 B** |        **1.00** |
| MediatorIMediator | 2         |  19.735 ns | 0.0421 ns | 0.0393 ns |  0.08 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  26.860 ns | 0.0104 ns | 0.0093 ns |  0.11 |      - |         - |        0.00 |
