```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         |  **90.636 ns** | **0.7137 ns** | **0.6676 ns** |  **1.00** | **0.0157** |     **264 B** |        **1.00** |
| MediatorIMediator | 0         |  11.278 ns | 0.0049 ns | 0.0046 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.976 ns | 0.0088 ns | 0.0083 ns |  0.03 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **229.688 ns** | **1.1734 ns** | **1.0402 ns** |  **1.00** | **0.0386** |     **648 B** |        **1.00** |
| MediatorIMediator | 2         |  17.105 ns | 0.0244 ns | 0.0228 ns |  0.07 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  25.261 ns | 0.0111 ns | 0.0099 ns |  0.11 |      - |         - |        0.00 |
