```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**                        | **False**   | **1.3206 ns** | **0.0017 ns** | **0.0013 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.4495 ns | 0.0047 ns | 0.0044 ns |  1.10 |         - |          NA |
| Typed_TwoErrors               | False   | 3.0765 ns | 0.0061 ns | 0.0057 ns |  2.33 |         - |          NA |
| Typed_ThreeErrors             | False   | 4.2107 ns | 0.0038 ns | 0.0032 ns |  3.19 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3362 ns | 0.0014 ns | 0.0011 ns |  1.01 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.7403 ns | 0.0023 ns | 0.0020 ns |  0.56 |         - |          NA |
|                               |         |           |           |           |       |           |             |
| **Result**                        | **True**    | **1.2264 ns** | **0.0074 ns** | **0.0066 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2201 ns | 0.0028 ns | 0.0026 ns |  0.99 |         - |          NA |
| Typed_TwoErrors               | True    | 2.6330 ns | 0.0038 ns | 0.0035 ns |  2.15 |         - |          NA |
| Typed_ThreeErrors             | True    | 3.3620 ns | 0.0051 ns | 0.0048 ns |  2.74 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.8142 ns | 0.0042 ns | 0.0039 ns |  0.66 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.4681 ns | 0.0006 ns | 0.0005 ns |  0.38 |         - |          NA |
