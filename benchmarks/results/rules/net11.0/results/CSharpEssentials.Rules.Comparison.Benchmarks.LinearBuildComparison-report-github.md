```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     |  **82.697 ns** | **0.5648 ns** | **0.5283 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 177.912 ns | 0.7481 ns | 0.6998 ns |  2.15 |    0.02 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 112.161 ns | 0.9259 ns | 0.8661 ns |  1.36 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   2.825 ns | 0.0057 ns | 0.0050 ns |  0.03 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **188.251 ns** | **3.2375 ns** | **2.7035 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 348.203 ns | 2.5095 ns | 2.2246 ns |  1.85 |    0.03 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 280.888 ns | 5.5239 ns | 6.9860 ns |  1.49 |    0.04 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   6.592 ns | 0.0117 ns | 0.0103 ns |  0.04 |    0.00 |      - |         - |          NA |
