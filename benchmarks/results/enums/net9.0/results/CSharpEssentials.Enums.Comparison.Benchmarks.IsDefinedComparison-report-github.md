```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **0.3748 ns** | **0.0078 ns** | **0.0065 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Bcl              | False   | 5.3400 ns | 0.0204 ns | 0.0181 ns | 14.25 |    0.24 |         - |          NA |
| EnumsNet         | False   | 1.3027 ns | 0.0023 ns | 0.0020 ns |  3.48 |    0.06 |         - |          NA |
| NetEscapades     | False   | 0.4524 ns | 0.0044 ns | 0.0042 ns |  1.21 |    0.02 |         - |          NA |
|                  |         |           |           |           |       |         |           |             |
| **CSharpEssentials** | **True**    | **0.3565 ns** | **0.0068 ns** | **0.0064 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Bcl              | True    | 5.7108 ns | 0.0077 ns | 0.0068 ns | 16.03 |    0.28 |         - |          NA |
| EnumsNet         | True    | 0.7048 ns | 0.0266 ns | 0.0249 ns |  1.98 |    0.08 |         - |          NA |
| NetEscapades     | True    | 0.7368 ns | 0.0010 ns | 0.0008 ns |  2.07 |    0.04 |         - |          NA |
