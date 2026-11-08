```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **81.619 ns** |  **0.1619 ns** |  **0.1515 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     2.241 ns |  0.0028 ns |  0.0022 ns |  0.03 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,151.643 ns | 27.2329 ns | 21.2617 ns | 38.61 |    0.26 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,490.528 ns | 13.4048 ns | 12.5389 ns | 18.26 |    0.15 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |    **93.996 ns** |  **0.2623 ns** |  **0.2453 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    10.712 ns |  0.0889 ns |  0.0832 ns |  0.11 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 4,820.420 ns | 34.6671 ns | 32.4277 ns | 51.28 |    0.36 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,645.927 ns |  8.4496 ns |  7.9037 ns | 17.51 |    0.09 | 0.2594 |      - |    4360 B |       77.86 |
