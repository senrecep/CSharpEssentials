```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |   **145.414 ns** |  **0.2040 ns** |  **0.1809 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.593 ns |  0.0070 ns |  0.0065 ns |  0.03 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,739.372 ns | 20.9999 ns | 19.6433 ns | 25.72 |    0.13 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 2,221.310 ns | 13.3021 ns | 11.7919 ns | 15.28 |    0.08 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **165.604 ns** |  **0.2236 ns** |  **0.2092 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    17.210 ns |  0.1060 ns |  0.0940 ns |  0.10 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,538.253 ns | 23.3017 ns | 21.7964 ns | 33.44 |    0.13 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,300.954 ns |  8.9322 ns |  7.9181 ns | 13.89 |    0.05 | 0.2632 |      - |    4416 B |       78.86 |
