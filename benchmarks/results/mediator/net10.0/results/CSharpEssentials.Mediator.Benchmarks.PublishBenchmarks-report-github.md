```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        | **103.404 ns** | **0.4101 ns** | **0.3635 ns** |  **1.00** | **0.0172** |     **288 B** |        **1.00** |
| MediatorIMediator | 1        |  12.300 ns | 0.0098 ns | 0.0087 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |   5.784 ns | 0.0063 ns | 0.0059 ns |  0.06 |      - |         - |        0.00 |
|                   |          |            |           |           |       |        |           |             |
| **MediatR**           | **5**        | **301.915 ns** | **1.6676 ns** | **1.4782 ns** |  **1.00** | **0.0534** |     **896 B** |        **1.00** |
| MediatorIMediator | 5        |  30.369 ns | 0.0310 ns | 0.0242 ns |  0.10 |      - |         - |        0.00 |
| MediatorConcrete  | 5        |  23.166 ns | 0.0124 ns | 0.0110 ns |  0.08 |      - |         - |        0.00 |
