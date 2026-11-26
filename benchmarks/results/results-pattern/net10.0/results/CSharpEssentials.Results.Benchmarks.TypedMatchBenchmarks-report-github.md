```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **Result**                        | **False**   | **1.4885 ns** | **0.0054 ns** | **0.0050 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.5245 ns | 0.0114 ns | 0.0107 ns |  1.02 |    0.01 |         - |          NA |
| Typed_TwoErrors               | False   | 3.3395 ns | 0.0026 ns | 0.0024 ns |  2.24 |    0.01 |         - |          NA |
| Typed_ThreeErrors             | False   | 4.2578 ns | 0.0027 ns | 0.0024 ns |  2.86 |    0.01 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3730 ns | 0.0026 ns | 0.0022 ns |  0.92 |    0.00 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.2589 ns | 0.0054 ns | 0.0048 ns |  0.17 |    0.00 |         - |          NA |
|                               |         |           |           |           |       |         |           |             |
| **Result**                        | **True**    | **1.2112 ns** | **0.0104 ns** | **0.0092 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2202 ns | 0.0052 ns | 0.0048 ns |  1.01 |    0.01 |         - |          NA |
| Typed_TwoErrors               | True    | 2.6347 ns | 0.0031 ns | 0.0028 ns |  2.18 |    0.02 |         - |          NA |
| Typed_ThreeErrors             | True    | 3.6846 ns | 0.0043 ns | 0.0039 ns |  3.04 |    0.02 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.9204 ns | 0.0081 ns | 0.0076 ns |  0.76 |    0.01 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.5069 ns | 0.0030 ns | 0.0028 ns |  0.42 |    0.00 |         - |          NA |
