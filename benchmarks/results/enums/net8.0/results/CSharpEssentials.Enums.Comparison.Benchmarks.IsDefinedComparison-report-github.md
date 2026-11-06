```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **0.4484 ns** | **0.0009 ns** | **0.0007 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Bcl              | False   | 5.0726 ns | 0.0132 ns | 0.0124 ns | 11.31 |    0.03 |         - |          NA |
| EnumsNet         | False   | 1.5039 ns | 0.0083 ns | 0.0078 ns |  3.35 |    0.02 |         - |          NA |
| NetEscapades     | False   | 0.3707 ns | 0.0062 ns | 0.0055 ns |  0.83 |    0.01 |         - |          NA |
|                  |         |           |           |           |       |         |           |             |
| **CSharpEssentials** | **True**    | **0.4483 ns** | **0.0121 ns** | **0.0113 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| Bcl              | True    | 5.1027 ns | 0.0184 ns | 0.0172 ns | 11.39 |    0.28 |         - |          NA |
| EnumsNet         | True    | 0.6926 ns | 0.0093 ns | 0.0087 ns |  1.55 |    0.04 |         - |          NA |
| NetEscapades     | True    | 0.4454 ns | 0.0088 ns | 0.0082 ns |  0.99 |    0.03 |         - |          NA |
