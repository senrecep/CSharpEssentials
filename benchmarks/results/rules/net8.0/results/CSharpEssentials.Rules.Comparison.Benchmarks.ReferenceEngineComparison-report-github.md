```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |   **116.021 ns** |  **0.1245 ns** |  **0.1040 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.485 ns |  0.0084 ns |  0.0079 ns |  0.04 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,761.612 ns | 15.3627 ns | 13.6187 ns | 32.42 |    0.12 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 2,159.329 ns | 13.1528 ns | 12.3031 ns | 18.61 |    0.10 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **133.622 ns** |  **0.5099 ns** |  **0.4770 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    17.189 ns |  0.1398 ns |  0.1307 ns |  0.13 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,538.396 ns | 37.9729 ns | 31.7091 ns | 41.45 |    0.27 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,390.721 ns | 12.7259 ns | 11.9038 ns | 17.89 |    0.11 | 0.2632 |      - |    4416 B |       78.86 |
