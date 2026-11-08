```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     |  **95.318 ns** | **0.7265 ns** | **0.6796 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 221.557 ns | 2.3171 ns | 2.1675 ns |  2.32 |    0.03 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 150.769 ns | 1.4656 ns | 1.3710 ns |  1.58 |    0.02 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.289 ns | 0.0113 ns | 0.0105 ns |  0.06 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **175.176 ns** | **0.2022 ns** | **0.1793 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 409.302 ns | 3.2547 ns | 3.0444 ns |  2.34 |    0.02 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 298.262 ns | 1.9560 ns | 1.8297 ns |  1.70 |    0.01 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   7.753 ns | 0.0134 ns | 0.0119 ns |  0.04 |    0.00 |      - |         - |          NA |
