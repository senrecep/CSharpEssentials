```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |   **116.115 ns** |  **0.1421 ns** |  **0.1260 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.482 ns |  0.0068 ns |  0.0061 ns |  0.04 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,728.061 ns | 20.9227 ns | 19.5712 ns | 32.11 |    0.17 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 2,153.313 ns | 10.0196 ns |  9.3724 ns | 18.54 |    0.08 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **133.305 ns** |  **0.4692 ns** |  **0.4389 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    18.030 ns |  0.1513 ns |  0.1341 ns |  0.14 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,429.812 ns | 31.2160 ns | 29.1995 ns | 40.73 |    0.25 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,321.007 ns | 13.8891 ns | 12.3123 ns | 17.41 |    0.11 | 0.2632 |      - |    4416 B |       78.86 |
