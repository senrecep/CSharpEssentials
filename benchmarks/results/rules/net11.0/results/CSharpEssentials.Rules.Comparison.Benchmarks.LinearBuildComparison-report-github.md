```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **101.984 ns** | **0.0395 ns** | **0.0308 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 196.179 ns | 0.6656 ns | 0.6226 ns |  1.92 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 138.419 ns | 1.4025 ns | 1.3119 ns |  1.36 |    0.01 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   2.834 ns | 0.0017 ns | 0.0014 ns |  0.03 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **177.017 ns** | **3.4059 ns** | **5.4999 ns** |  **1.00** |    **0.04** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 391.407 ns | 2.7027 ns | 2.5281 ns |  2.21 |    0.06 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 282.208 ns | 1.0404 ns | 0.9732 ns |  1.60 |    0.05 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   6.633 ns | 0.0498 ns | 0.0465 ns |  0.04 |    0.00 |      - |         - |          NA |
