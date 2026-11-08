```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **123.356 ns** | **0.2127 ns** | **0.1989 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 254.771 ns | 1.8434 ns | 1.6341 ns |  2.07 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 183.862 ns | 0.8934 ns | 0.7919 ns |  1.49 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.254 ns | 0.0094 ns | 0.0078 ns |  0.04 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **246.020 ns** | **0.3488 ns** | **0.3092 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 517.347 ns | 4.7711 ns | 4.4629 ns |  2.10 |    0.02 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 360.600 ns | 1.7620 ns | 1.6482 ns |  1.47 |    0.01 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  12.785 ns | 0.0181 ns | 0.0151 ns |  0.05 |    0.00 |      - |         - |          NA |
