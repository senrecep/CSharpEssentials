```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **58.080 ns** |  **0.0835 ns** |  **0.0781 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     2.245 ns |  0.0068 ns |  0.0060 ns |  0.04 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,230.602 ns | 19.8819 ns | 17.6248 ns | 55.62 |    0.30 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,489.506 ns | 11.5503 ns | 10.8042 ns | 25.65 |    0.18 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |    **67.526 ns** |  **0.2912 ns** |  **0.2581 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    11.556 ns |  0.0936 ns |  0.0876 ns |  0.17 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,156.951 ns | 27.2334 ns | 24.1417 ns | 76.37 |    0.45 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,637.418 ns |  4.3575 ns |  3.4021 ns | 24.25 |    0.10 | 0.2594 |      - |    4360 B |       77.86 |
