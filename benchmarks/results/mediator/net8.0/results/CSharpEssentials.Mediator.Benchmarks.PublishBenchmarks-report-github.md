```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        | **119.58 ns** | **0.667 ns** | **0.591 ns** |  **1.00** | **0.0172** |     **288 B** |        **1.00** |
| MediatorIMediator | 1        |  40.65 ns | 0.065 ns | 0.061 ns |  0.34 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |  28.71 ns | 0.020 ns | 0.018 ns |  0.24 |      - |         - |        0.00 |
|                   |          |           |          |          |       |        |           |             |
| **MediatR**           | **5**        | **350.04 ns** | **2.453 ns** | **2.294 ns** |  **1.00** | **0.0534** |     **896 B** |        **1.00** |
| MediatorIMediator | 5        | 146.90 ns | 1.112 ns | 1.041 ns |  0.42 |      - |         - |        0.00 |
| MediatorConcrete  | 5        | 140.07 ns | 1.195 ns | 1.060 ns |  0.40 |      - |         - |        0.00 |
