```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |   **130.086 ns** |  **0.0616 ns** |  **0.0546 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.461 ns |  0.0041 ns |  0.0034 ns |  0.03 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,791.449 ns | 17.3383 ns | 16.2182 ns | 29.15 |    0.12 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 2,050.586 ns |  8.4506 ns |  7.9047 ns | 15.76 |    0.06 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **135.163 ns** |  **0.1778 ns** |  **0.1576 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    16.641 ns |  0.0940 ns |  0.0880 ns |  0.12 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,468.200 ns | 22.6869 ns | 20.1113 ns | 40.46 |    0.15 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,249.812 ns |  7.2305 ns |  6.4096 ns | 16.65 |    0.05 | 0.2632 |      - |    4416 B |       78.86 |
