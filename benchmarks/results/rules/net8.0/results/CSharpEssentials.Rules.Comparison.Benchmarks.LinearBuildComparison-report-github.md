```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **116.687 ns** | **0.1380 ns** | **0.1078 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 287.019 ns | 1.3844 ns | 1.1561 ns |  2.46 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 179.833 ns | 0.8022 ns | 0.7111 ns |  1.54 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.029 ns | 0.0117 ns | 0.0110 ns |  0.04 |      - |         - |          NA |
|                  |       |            |           |           |       |        |           |             |
| **Prebuilt**         | **10**    | **256.821 ns** | **0.3622 ns** | **0.3025 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 578.811 ns | 3.9209 ns | 3.6676 ns |  2.25 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 395.115 ns | 3.1299 ns | 2.9278 ns |  1.54 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  13.270 ns | 0.0244 ns | 0.0228 ns |  0.05 |      - |         - |          NA |
