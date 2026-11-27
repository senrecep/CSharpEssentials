```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**                        | **False**   | **1.3747 ns** | **0.0041 ns** | **0.0038 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.4620 ns | 0.0034 ns | 0.0030 ns |  1.06 |         - |          NA |
| Typed_TwoErrors               | False   | 1.9028 ns | 0.0039 ns | 0.0034 ns |  1.38 |         - |          NA |
| Typed_ThreeErrors             | False   | 2.5278 ns | 0.0022 ns | 0.0018 ns |  1.84 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3732 ns | 0.0026 ns | 0.0023 ns |  1.00 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.1702 ns | 0.0019 ns | 0.0016 ns |  0.12 |         - |          NA |
|                               |         |           |           |           |       |           |             |
| **Result**                        | **True**    | **1.0852 ns** | **0.0025 ns** | **0.0024 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2261 ns | 0.0053 ns | 0.0050 ns |  1.13 |         - |          NA |
| Typed_TwoErrors               | True    | 1.5837 ns | 0.0053 ns | 0.0049 ns |  1.46 |         - |          NA |
| Typed_ThreeErrors             | True    | 2.2387 ns | 0.0044 ns | 0.0039 ns |  2.06 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.8250 ns | 0.0046 ns | 0.0041 ns |  0.76 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.5064 ns | 0.0031 ns | 0.0026 ns |  0.47 |         - |          NA |
