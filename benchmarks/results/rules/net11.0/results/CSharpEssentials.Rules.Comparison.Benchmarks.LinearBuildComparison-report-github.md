```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     |  **72.662 ns** | **0.0834 ns** | **0.0739 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 165.412 ns | 0.3909 ns | 0.3466 ns |  2.28 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 112.424 ns | 1.3472 ns | 1.1942 ns |  1.55 |    0.02 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   2.822 ns | 0.0065 ns | 0.0057 ns |  0.04 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **186.165 ns** | **3.6033 ns** | **4.1496 ns** |  **1.00** |    **0.03** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 361.068 ns | 6.2691 ns | 5.8641 ns |  1.94 |    0.05 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 258.102 ns | 2.5619 ns | 2.2710 ns |  1.39 |    0.03 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   6.595 ns | 0.0165 ns | 0.0147 ns |  0.04 |    0.00 |      - |         - |          NA |
