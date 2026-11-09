```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **99.433 ns** |  **0.2446 ns** |  **0.2169 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.476 ns |  0.0087 ns |  0.0077 ns |  0.05 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,654.437 ns | 24.4573 ns | 22.8774 ns | 36.75 |    0.24 | 0.6561 | 0.0153 |   10984 B |          NA |
| RulesEngineReference | Pass     | 2,063.821 ns | 11.0218 ns |  9.7705 ns | 20.76 |    0.10 | 0.2556 |      - |    4296 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |   **111.822 ns** |  **0.2532 ns** |  **0.2245 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    16.166 ns |  0.1306 ns |  0.1222 ns |  0.14 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 5,348.921 ns | 35.6000 ns | 33.3003 ns | 47.83 |    0.30 | 0.8316 | 0.0229 |   14016 B |      250.29 |
| RulesEngineReference | FailLast | 2,242.292 ns | 15.0929 ns | 13.3794 ns | 20.05 |    0.12 | 0.2632 |      - |    4416 B |       78.86 |
