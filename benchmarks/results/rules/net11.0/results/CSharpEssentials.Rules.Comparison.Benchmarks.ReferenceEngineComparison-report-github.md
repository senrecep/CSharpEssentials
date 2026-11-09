```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **63.309 ns** |  **0.0716 ns** |  **0.0634 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     2.247 ns |  0.0116 ns |  0.0103 ns |  0.04 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,146.639 ns | 50.0048 ns | 46.7745 ns | 49.70 |    0.72 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,545.836 ns |  6.4257 ns |  5.3657 ns | 24.42 |    0.09 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |    **68.537 ns** |  **0.3262 ns** |  **0.3051 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    10.860 ns |  0.0832 ns |  0.0649 ns |  0.16 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 4,669.467 ns | 23.9043 ns | 22.3601 ns | 68.13 |    0.43 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,605.936 ns |  6.8248 ns |  6.3840 ns | 23.43 |    0.14 | 0.2594 |      - |    4360 B |       77.86 |
