```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**                        | **False**   | **1.3746 ns** | **0.0044 ns** | **0.0041 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.4646 ns | 0.0041 ns | 0.0038 ns |  1.07 |         - |          NA |
| Typed_TwoErrors               | False   | 3.1122 ns | 0.0090 ns | 0.0084 ns |  2.26 |         - |          NA |
| Typed_ThreeErrors             | False   | 4.2074 ns | 0.0022 ns | 0.0018 ns |  3.06 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3727 ns | 0.0022 ns | 0.0019 ns |  1.00 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.2905 ns | 0.0020 ns | 0.0019 ns |  0.21 |         - |          NA |
|                               |         |           |           |           |       |           |             |
| **Result**                        | **True**    | **1.0848 ns** | **0.0024 ns** | **0.0022 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2284 ns | 0.0062 ns | 0.0058 ns |  1.13 |         - |          NA |
| Typed_TwoErrors               | True    | 2.6655 ns | 0.0033 ns | 0.0029 ns |  2.46 |         - |          NA |
| Typed_ThreeErrors             | True    | 3.3956 ns | 0.0025 ns | 0.0021 ns |  3.13 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.8562 ns | 0.0047 ns | 0.0042 ns |  0.79 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.5055 ns | 0.0009 ns | 0.0008 ns |  0.47 |         - |          NA |
