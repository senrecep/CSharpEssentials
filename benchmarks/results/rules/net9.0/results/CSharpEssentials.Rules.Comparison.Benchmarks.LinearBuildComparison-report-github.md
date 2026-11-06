```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **125.812 ns** | **0.1257 ns** | **0.0981 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 258.934 ns | 2.3284 ns | 2.1780 ns |  2.06 |    0.02 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 169.750 ns | 0.8844 ns | 0.8273 ns |  1.35 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.281 ns | 0.0185 ns | 0.0173 ns |  0.04 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **244.807 ns** | **1.9675 ns** | **1.8404 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 522.430 ns | 1.4465 ns | 1.3530 ns |  2.13 |    0.02 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 363.571 ns | 1.1174 ns | 0.9906 ns |  1.49 |    0.01 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  13.716 ns | 0.0357 ns | 0.0334 ns |  0.06 |    0.00 |      - |         - |          NA |
