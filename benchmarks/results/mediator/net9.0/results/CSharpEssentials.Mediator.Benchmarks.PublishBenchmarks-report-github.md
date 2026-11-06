```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **MediatR**           | **1**        | **113.90 ns** | **0.527 ns** | **0.467 ns** |  **1.00** | **0.0172** |     **288 B** |        **1.00** |
| MediatorIMediator | 1        |  27.65 ns | 0.007 ns | 0.007 ns |  0.24 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |  15.75 ns | 0.011 ns | 0.009 ns |  0.14 |      - |         - |        0.00 |
|                   |          |           |          |          |       |        |           |             |
| **MediatR**           | **5**        | **366.92 ns** | **1.906 ns** | **1.690 ns** |  **1.00** | **0.0534** |     **896 B** |        **1.00** |
| MediatorIMediator | 5        |  55.41 ns | 0.047 ns | 0.044 ns |  0.15 |      - |         - |        0.00 |
| MediatorConcrete  | 5        |  39.42 ns | 0.077 ns | 0.068 ns |  0.11 |      - |         - |        0.00 |
