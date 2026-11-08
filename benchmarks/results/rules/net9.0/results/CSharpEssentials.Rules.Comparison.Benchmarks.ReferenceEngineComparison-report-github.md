```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **99.992 ns** |  **0.1659 ns** |  **0.1385 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.602 ns |  0.0068 ns |  0.0060 ns |  0.05 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,722.919 ns | 53.3268 ns | 47.2728 ns | 37.23 |    0.46 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 1,981.387 ns | 13.8544 ns | 12.9594 ns | 19.82 |    0.13 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **113.096 ns** |  **0.3419 ns** |  **0.3198 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    17.283 ns |  0.2444 ns |  0.2041 ns |  0.15 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,185.864 ns | 41.7222 ns | 36.9857 ns | 45.85 |    0.34 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,198.416 ns | 15.1437 ns | 14.1655 ns | 19.44 |    0.13 | 0.2632 |      - |    4416 B |       78.86 |
