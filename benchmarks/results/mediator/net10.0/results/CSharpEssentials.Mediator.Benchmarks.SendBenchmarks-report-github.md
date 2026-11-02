```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         |  **67.444 ns** | **0.5281 ns** | **0.4940 ns** |  **1.00** | **0.0119** |     **200 B** |        **1.00** |
| MediatorIMediator | 0         |   7.799 ns | 0.0068 ns | 0.0061 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   3.344 ns | 0.0036 ns | 0.0034 ns |  0.05 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **180.570 ns** | **1.0932 ns** | **1.0226 ns** |  **1.00** | **0.0348** |     **584 B** |        **1.00** |
| MediatorIMediator | 2         |  31.110 ns | 0.0393 ns | 0.0368 ns |  0.17 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  14.825 ns | 0.0428 ns | 0.0400 ns |  0.08 |      - |         - |        0.00 |
