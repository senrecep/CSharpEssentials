```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |---------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   | **4.002 ns** | **0.0107 ns** | **0.0095 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   | 4.937 ns | 0.0129 ns | 0.0114 ns |  1.23 |         - |          NA |
| Typed_TwoErrors   | False   | 5.118 ns | 0.0166 ns | 0.0139 ns |  1.28 |         - |          NA |
| Typed_ThreeErrors | False   | 5.038 ns | 0.0145 ns | 0.0136 ns |  1.26 |         - |          NA |
|                   |         |          |           |           |       |           |             |
| **Result**            | **True**    | **3.603 ns** | **0.0058 ns** | **0.0048 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 4.319 ns | 0.0098 ns | 0.0092 ns |  1.20 |         - |          NA |
| Typed_TwoErrors   | True    | 4.038 ns | 0.0138 ns | 0.0129 ns |  1.12 |         - |          NA |
| Typed_ThreeErrors | True    | 4.405 ns | 0.0084 ns | 0.0071 ns |  1.22 |         - |          NA |
