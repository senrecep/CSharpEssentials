```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     |  **91.197 ns** | **0.1616 ns** | **0.1433 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 190.275 ns | 1.0881 ns | 1.0178 ns |  2.09 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 133.359 ns | 1.1878 ns | 1.0530 ns |  1.46 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   2.821 ns | 0.0049 ns | 0.0038 ns |  0.03 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **172.476 ns** | **0.2947 ns** | **0.2612 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 393.064 ns | 4.1533 ns | 3.8850 ns |  2.28 |    0.02 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 265.187 ns | 1.9944 ns | 1.8656 ns |  1.54 |    0.01 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   6.588 ns | 0.0093 ns | 0.0078 ns |  0.04 |    0.00 |      - |         - |          NA |
