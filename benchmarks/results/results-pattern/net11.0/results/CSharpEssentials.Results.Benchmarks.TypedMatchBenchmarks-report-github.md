```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**                        | **False**   | **1.3135 ns** | **0.0040 ns** | **0.0034 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.5444 ns | 0.0107 ns | 0.0100 ns |  1.18 |         - |          NA |
| Typed_TwoErrors               | False   | 1.9171 ns | 0.0032 ns | 0.0030 ns |  1.46 |         - |          NA |
| Typed_ThreeErrors             | False   | 1.9130 ns | 0.0033 ns | 0.0029 ns |  1.46 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3358 ns | 0.0028 ns | 0.0025 ns |  1.02 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.2541 ns | 0.0061 ns | 0.0057 ns |  0.19 |         - |          NA |
|                               |         |           |           |           |       |           |             |
| **Result**                        | **True**    | **1.2120 ns** | **0.0082 ns** | **0.0077 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2243 ns | 0.0050 ns | 0.0044 ns |  1.01 |         - |          NA |
| Typed_TwoErrors               | True    | 1.2954 ns | 0.0045 ns | 0.0042 ns |  1.07 |         - |          NA |
| Typed_ThreeErrors             | True    | 1.6981 ns | 0.0122 ns | 0.0114 ns |  1.40 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 1.0510 ns | 0.0113 ns | 0.0106 ns |  0.87 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.5050 ns | 0.0008 ns | 0.0007 ns |  0.42 |         - |          NA |
