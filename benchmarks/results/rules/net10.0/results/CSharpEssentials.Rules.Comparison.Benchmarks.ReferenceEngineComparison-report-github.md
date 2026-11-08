```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method               | Input    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CseLinear**            | **Pass**     |    **86.860 ns** |  **0.1605 ns** |  **0.1501 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| HandWritten          | Pass     |     4.613 ns |  0.0067 ns |  0.0059 ns |  0.05 |    0.00 |      - |      - |         - |          NA |
| NRulesReference      | Pass     | 3,291.501 ns | 11.0891 ns |  9.8302 ns | 37.89 |    0.13 | 0.6485 | 0.0153 |   10872 B |          NA |
| RulesEngineReference | Pass     | 1,537.237 ns |  8.2140 ns |  7.6834 ns | 17.70 |    0.09 | 0.2518 | 0.0019 |    4240 B |          NA |
|                      |          |              |            |            |       |         |        |        |           |             |
| **CseLinear**            | **FailLast** |    **99.306 ns** |  **0.4597 ns** |  **0.4075 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| HandWritten          | FailLast |    15.963 ns |  0.1204 ns |  0.1126 ns |  0.16 |    0.00 | 0.0033 |      - |      56 B |        1.00 |
| NRulesReference      | FailLast | 4,982.426 ns | 37.2654 ns | 34.8581 ns | 50.17 |    0.39 | 0.8240 | 0.0229 |   13864 B |      247.57 |
| RulesEngineReference | FailLast | 1,683.337 ns |  7.3110 ns |  6.8387 ns | 16.95 |    0.09 | 0.2594 |      - |    4360 B |       77.86 |
