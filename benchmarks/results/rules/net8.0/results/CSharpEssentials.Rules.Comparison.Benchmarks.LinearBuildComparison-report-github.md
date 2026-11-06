```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **137.046 ns** | **0.1807 ns** | **0.1690 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 302.857 ns | 2.6491 ns | 2.4780 ns |  2.21 |    0.02 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 203.861 ns | 0.2937 ns | 0.2748 ns |  1.49 |    0.00 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.014 ns | 0.0039 ns | 0.0034 ns |  0.04 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **290.608 ns** | **5.7193 ns** | **6.8084 ns** |  **1.00** |    **0.03** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 612.153 ns | 6.2074 ns | 5.8064 ns |  2.11 |    0.05 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 412.810 ns | 1.1182 ns | 1.0459 ns |  1.42 |    0.03 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  13.220 ns | 0.0183 ns | 0.0163 ns |  0.05 |    0.00 |      - |         - |          NA |
