```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        | **104.059 ns** | **0.6517 ns** | **0.6096 ns** |  **1.00** | **0.0172** |     **288 B** |        **1.00** |
| MediatorIMediator | 1        |  12.465 ns | 0.0150 ns | 0.0140 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |   5.779 ns | 0.0180 ns | 0.0169 ns |  0.06 |      - |         - |        0.00 |
|                   |          |            |           |           |       |        |           |             |
| **MediatR**           | **5**        | **311.734 ns** | **2.7748 ns** | **2.5956 ns** |  **1.00** | **0.0534** |     **896 B** |        **1.00** |
| MediatorIMediator | 5        |  30.013 ns | 0.0200 ns | 0.0187 ns |  0.10 |      - |         - |        0.00 |
| MediatorConcrete  | 5        |  23.805 ns | 0.0271 ns | 0.0253 ns |  0.08 |      - |         - |        0.00 |
