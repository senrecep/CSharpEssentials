```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |   **138.196 ns** |  **0.6233 ns** |  **0.5205 ns** |  **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.483 ns |  0.0075 ns |  0.0070 ns |  0.03 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,797.871 ns | 22.2228 ns | 20.7872 ns | 27.48 |    0.18 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 2,176.761 ns | 42.0593 ns | 37.2845 ns | 15.75 |    0.27 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **155.026 ns** |  **0.9745 ns** |  **0.8137 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    17.943 ns |  0.1686 ns |  0.1577 ns |  0.12 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,519.060 ns | 83.1478 ns | 73.7083 ns | 35.60 |    0.49 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,325.710 ns | 19.1676 ns | 17.9294 ns | 15.00 |    0.14 | 0.2632 |      - |    4416 B |       78.86 |
