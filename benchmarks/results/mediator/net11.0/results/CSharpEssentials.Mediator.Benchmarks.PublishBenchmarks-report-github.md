```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        |  **99.779 ns** | **0.2756 ns** | **0.2302 ns** |  **1.00** | **0.0162** |     **272 B** |        **1.00** |
| MediatorIMediator | 1        |  12.416 ns | 0.0540 ns | 0.0505 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |   6.196 ns | 0.0287 ns | 0.0255 ns |  0.06 |      - |         - |        0.00 |
|                   |          |            |           |           |       |        |           |             |
| **MediatR**           | **5**        | **298.645 ns** | **1.6500 ns** | **1.5434 ns** |  **1.00** | **0.0505** |     **848 B** |        **1.00** |
| MediatorIMediator | 5        |  34.830 ns | 0.0481 ns | 0.0402 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 5        |  23.559 ns | 0.0161 ns | 0.0142 ns |  0.08 |      - |         - |        0.00 |
