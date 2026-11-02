```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        |  **97.243 ns** | **0.5543 ns** | **0.5185 ns** |  **1.00** | **0.0162** |     **272 B** |        **1.00** |
| MediatorIMediator | 1        |  12.601 ns | 0.0180 ns | 0.0168 ns |  0.13 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |   6.227 ns | 0.0058 ns | 0.0049 ns |  0.06 |      - |         - |        0.00 |
|                   |          |            |           |           |       |        |           |             |
| **MediatR**           | **5**        | **278.984 ns** | **1.5953 ns** | **1.4142 ns** |  **1.00** | **0.0505** |     **848 B** |        **1.00** |
| MediatorIMediator | 5        |  30.077 ns | 0.0543 ns | 0.0481 ns |  0.11 |      - |         - |        0.00 |
| MediatorConcrete  | 5        |  24.324 ns | 0.0272 ns | 0.0255 ns |  0.09 |      - |         - |        0.00 |
