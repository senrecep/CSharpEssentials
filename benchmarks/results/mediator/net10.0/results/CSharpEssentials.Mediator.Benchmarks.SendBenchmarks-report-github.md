```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method            | Behaviors | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **MediatR**           | **0**         |  **68.270 ns** | **0.3274 ns** | **0.2902 ns** |  **1.00** | **0.0119** |     **200 B** |        **1.00** |
| MediatorIMediator | 0         |   7.761 ns | 0.0042 ns | 0.0039 ns |  0.11 |      - |         - |        0.00 |
| MediatorConcrete  | 0         |   2.886 ns | 0.0065 ns | 0.0057 ns |  0.04 |      - |         - |        0.00 |
|                   |           |            |           |           |       |        |           |             |
| **MediatR**           | **2**         | **185.857 ns** | **0.8893 ns** | **0.8318 ns** |  **1.00** | **0.0348** |     **584 B** |        **1.00** |
| MediatorIMediator | 2         |  26.152 ns | 0.0120 ns | 0.0100 ns |  0.14 |      - |         - |        0.00 |
| MediatorConcrete  | 2         |  13.967 ns | 0.0094 ns | 0.0083 ns |  0.08 |      - |         - |        0.00 |
