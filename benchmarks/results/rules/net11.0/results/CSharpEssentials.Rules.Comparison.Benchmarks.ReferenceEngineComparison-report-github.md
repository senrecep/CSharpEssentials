```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **87.452 ns** |  **0.3517 ns** |  **0.3290 ns** |  **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     2.285 ns |  0.0195 ns |  0.0182 ns |  0.03 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,452.054 ns | 28.7248 ns | 26.8692 ns | 39.47 |    0.33 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,525.928 ns |  8.4456 ns |  7.9001 ns | 17.45 |    0.11 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |    **97.123 ns** |  **0.1068 ns** |  **0.0892 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    11.465 ns |  0.1496 ns |  0.1400 ns |  0.12 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 4,865.466 ns | 28.6967 ns | 26.8429 ns | 50.10 |    0.27 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,676.275 ns | 10.2418 ns |  9.5802 ns | 17.26 |    0.10 | 0.2594 |      - |    4360 B |       77.86 |
