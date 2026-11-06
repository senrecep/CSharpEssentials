```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         |  **66.601 ns** | **0.4287 ns** | **0.4010 ns** |  **1.00** | **0.0114** |     **192 B** |        **1.00** |
| MediatorIMediator | 0         |   7.762 ns | 0.0063 ns | 0.0049 ns |  0.12 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.799 ns | 0.0107 ns | 0.0100 ns |  0.04 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **174.991 ns** | **1.4358 ns** | **1.3430 ns** |  **1.00** | **0.0334** |     **560 B** |        **1.00** |
| MediatorIMediator | 2         |  31.187 ns | 0.0410 ns | 0.0383 ns |  0.18 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  14.937 ns | 0.0265 ns | 0.0248 ns |  0.09 |      - |         - |        0.00 |
