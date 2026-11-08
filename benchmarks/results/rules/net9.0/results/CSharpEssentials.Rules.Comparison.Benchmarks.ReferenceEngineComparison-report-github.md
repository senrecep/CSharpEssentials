```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |   **123.760 ns** |  **0.1497 ns** |  **0.1250 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.596 ns |  0.0042 ns |  0.0040 ns |  0.04 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,751.038 ns | 26.8206 ns | 25.0880 ns | 30.31 |    0.20 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 1,987.713 ns | 20.0866 ns | 18.7890 ns | 16.06 |    0.15 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **138.558 ns** |  **0.4410 ns** |  **0.4125 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    16.321 ns |  0.1245 ns |  0.1164 ns |  0.12 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,366.300 ns | 34.5391 ns | 32.3079 ns | 38.73 |    0.25 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,185.585 ns | 12.3820 ns | 11.5821 ns | 15.77 |    0.09 | 0.2632 |      - |    4416 B |       78.86 |
