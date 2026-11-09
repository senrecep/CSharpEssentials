```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **101.655 ns** | **0.1275 ns** | **0.1192 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 242.523 ns | 1.4734 ns | 1.3782 ns |  2.39 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 153.666 ns | 1.3302 ns | 1.1792 ns |  1.51 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.246 ns | 0.0086 ns | 0.0081 ns |  0.05 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **219.249 ns** | **0.2621 ns** | **0.2323 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 495.101 ns | 4.0779 ns | 3.8145 ns |  2.26 |    0.02 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 370.070 ns | 1.8258 ns | 1.7079 ns |  1.69 |    0.01 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  12.875 ns | 0.0254 ns | 0.0238 ns |  0.06 |    0.00 |      - |         - |          NA |
