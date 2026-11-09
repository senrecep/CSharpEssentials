```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **80.364 ns** |  **0.1893 ns** |  **0.1771 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.610 ns |  0.0102 ns |  0.0096 ns |  0.06 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,289.964 ns | 12.6838 ns | 11.8645 ns | 40.94 |    0.17 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,566.710 ns | 10.5763 ns |  9.8931 ns | 19.50 |    0.13 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |    **75.405 ns** |  **0.2772 ns** |  **0.2593 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    15.952 ns |  0.1076 ns |  0.1007 ns |  0.21 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,044.944 ns | 26.7086 ns | 20.8523 ns | 66.90 |    0.35 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,697.992 ns |  5.8105 ns |  5.4352 ns | 22.52 |    0.10 | 0.2594 |      - |    4360 B |       77.86 |
