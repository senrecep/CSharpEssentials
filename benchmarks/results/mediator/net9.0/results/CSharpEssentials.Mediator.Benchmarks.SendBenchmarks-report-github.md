```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         |  **88.508 ns** | **0.8460 ns** | **0.7913 ns** |  **1.00** | **0.0157** |     **264 B** |        **1.00** |
| MediatorIMediator | 0         |  12.163 ns | 0.0132 ns | 0.0123 ns |  0.14 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.970 ns | 0.0207 ns | 0.0194 ns |  0.03 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **222.907 ns** | **2.5094 ns** | **2.3473 ns** |  **1.00** | **0.0386** |     **648 B** |        **1.00** |
| MediatorIMediator | 2         |  17.533 ns | 0.0803 ns | 0.0751 ns |  0.08 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  25.241 ns | 0.0065 ns | 0.0054 ns |  0.11 |      - |         - |        0.00 |
