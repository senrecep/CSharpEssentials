```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **MediatR**           | **0**         | **132.252 ns** | **0.8603 ns** | **0.8047 ns** |  **1.00** |    **0.01** | **0.0186** |     **312 B** |        **1.00** |
| MediatorIMediator | 0         |  11.254 ns | 0.0078 ns | 0.0073 ns |  0.09 |    0.00 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.979 ns | 0.0168 ns | 0.0157 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                   |           |            |           |           |       |         |        |           |             |
| **MediatR**           | **2**         | **249.812 ns** | **4.9167 ns** | **4.5991 ns** |  **1.00** |    **0.03** | **0.0386** |     **648 B** |        **1.00** |
| MediatorIMediator | 2         |  18.630 ns | 0.0579 ns | 0.0542 ns |  0.07 |    0.00 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  26.793 ns | 0.0161 ns | 0.0143 ns |  0.11 |    0.00 |      - |         - |        0.00 |
