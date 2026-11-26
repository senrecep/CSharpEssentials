```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |---------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   | **3.992 ns** | **0.0126 ns** | **0.0118 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   | 5.053 ns | 0.0136 ns | 0.0127 ns |  1.27 |         - |          NA |
| Typed_TwoErrors   | False   | 5.116 ns | 0.0141 ns | 0.0132 ns |  1.28 |         - |          NA |
| Typed_ThreeErrors | False   | 5.025 ns | 0.0137 ns | 0.0128 ns |  1.26 |         - |          NA |
|                   |         |          |           |           |       |           |             |
| **Result**            | **True**    | **4.175 ns** | **0.0033 ns** | **0.0026 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 4.429 ns | 0.0094 ns | 0.0088 ns |  1.06 |         - |          NA |
| Typed_TwoErrors   | True    | 4.497 ns | 0.0182 ns | 0.0170 ns |  1.08 |         - |          NA |
| Typed_ThreeErrors | True    | 4.576 ns | 0.0056 ns | 0.0053 ns |  1.10 |         - |          NA |
