```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method            | Handlers | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |--------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **MediatR**           | **1**        | **115.48 ns** | **2.282 ns** | **2.803 ns** |  **1.00** |    **0.03** | **0.0172** |     **288 B** |        **1.00** |
| MediatorIMediator | 1        |  28.31 ns | 0.017 ns | 0.014 ns |  0.25 |    0.01 |      - |         - |        0.00 |
| MediatorConcrete  | 1        |  15.76 ns | 0.020 ns | 0.018 ns |  0.14 |    0.00 |      - |         - |        0.00 |
|                   |          |           |          |          |       |         |        |           |             |
| **MediatR**           | **5**        | **331.46 ns** | **3.269 ns** | **3.058 ns** |  **1.00** |    **0.01** | **0.0534** |     **896 B** |        **1.00** |
| MediatorIMediator | 5        |  50.84 ns | 0.023 ns | 0.019 ns |  0.15 |    0.00 |      - |         - |        0.00 |
| MediatorConcrete  | 5        |  39.67 ns | 0.035 ns | 0.031 ns |  0.12 |    0.00 |      - |         - |        0.00 |
