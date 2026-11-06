```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        | **124.79 ns** | **0.591 ns** | **0.553 ns** |  **1.00** | **0.0172** |     **288 B** |        **1.00** |
| MediatorIMediator | 1        |  40.51 ns | 0.041 ns | 0.034 ns |  0.32 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |  26.44 ns | 0.006 ns | 0.005 ns |  0.21 |      - |         - |        0.00 |
|                   |          |           |          |          |       |        |           |             |
| **MediatR**           | **5**        | **364.75 ns** | **1.735 ns** | **1.449 ns** |  **1.00** | **0.0534** |     **896 B** |        **1.00** |
| MediatorIMediator | 5        | 153.09 ns | 0.707 ns | 0.661 ns |  0.42 |      - |         - |        0.00 |
| MediatorConcrete  | 5        | 143.13 ns | 0.629 ns | 0.588 ns |  0.39 |      - |         - |        0.00 |
