```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **116.335 ns** | **0.1442 ns** | **0.1278 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 284.706 ns | 2.5651 ns | 2.3994 ns |  2.45 |    0.02 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 175.910 ns | 0.9083 ns | 0.8496 ns |  1.51 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.015 ns | 0.0075 ns | 0.0070 ns |  0.04 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **284.153 ns** | **4.4934 ns** | **3.7522 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 613.439 ns | 4.7522 ns | 4.4452 ns |  2.16 |    0.03 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 400.395 ns | 1.8556 ns | 1.6450 ns |  1.41 |    0.02 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  13.198 ns | 0.0211 ns | 0.0187 ns |  0.05 |    0.00 |      - |         - |          NA |
