```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **138.332 ns** | **0.3413 ns** | **0.3192 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 383.711 ns | 1.0164 ns | 0.9508 ns |  2.77 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 205.121 ns | 0.6340 ns | 0.5620 ns |  1.48 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.490 ns | 0.0061 ns | 0.0057 ns |  0.04 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **313.322 ns** | **6.2496 ns** | **8.7610 ns** |  **1.00** |    **0.04** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 654.054 ns | 4.7842 ns | 4.2411 ns |  2.09 |    0.06 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 435.783 ns | 5.5661 ns | 5.2065 ns |  1.39 |    0.04 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  13.380 ns | 0.0207 ns | 0.0173 ns |  0.04 |    0.00 |      - |         - |          NA |
