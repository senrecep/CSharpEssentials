```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **87.259 ns** |  **0.0531 ns** |  **0.0471 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.596 ns |  0.0033 ns |  0.0028 ns |  0.05 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,515.930 ns | 19.8954 ns | 18.6102 ns | 40.29 |    0.21 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,610.556 ns |  5.0063 ns |  4.6829 ns | 18.46 |    0.05 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **103.876 ns** |  **0.2432 ns** |  **0.2275 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    16.490 ns |  0.0491 ns |  0.0459 ns |  0.16 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,003.017 ns | 62.0038 ns | 57.9984 ns | 48.16 |    0.55 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,727.368 ns |  7.2884 ns |  6.8176 ns | 16.63 |    0.07 | 0.2594 |      - |    4360 B |       77.86 |
