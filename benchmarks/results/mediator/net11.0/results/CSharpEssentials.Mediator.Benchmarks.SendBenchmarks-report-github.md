```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         |  **63.248 ns** | **0.6010 ns** | **0.5621 ns** |  **1.00** | **0.0114** |     **192 B** |        **1.00** |
| MediatorIMediator | 0         |   7.781 ns | 0.0079 ns | 0.0074 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.882 ns | 0.0066 ns | 0.0059 ns |  0.05 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **173.777 ns** | **1.1019 ns** | **1.0307 ns** |  **1.00** | **0.0334** |     **560 B** |        **1.00** |
| MediatorIMediator | 2         |  21.272 ns | 0.0200 ns | 0.0187 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  14.939 ns | 0.0262 ns | 0.0245 ns |  0.09 |      - |         - |        0.00 |
