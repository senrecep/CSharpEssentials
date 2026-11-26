```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   |  **7.100 ns** | **0.0192 ns** | **0.0180 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   |  9.574 ns | 0.0064 ns | 0.0057 ns |  1.35 |         - |          NA |
| Typed_TwoErrors   | False   |  9.565 ns | 0.0070 ns | 0.0066 ns |  1.35 |         - |          NA |
| Typed_ThreeErrors | False   |  9.335 ns | 0.0701 ns | 0.0656 ns |  1.31 |         - |          NA |
|                   |         |           |           |           |       |           |             |
| **Result**            | **True**    |  **5.506 ns** | **0.0094 ns** | **0.0088 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 19.635 ns | 0.0166 ns | 0.0155 ns |  3.57 |         - |          NA |
| Typed_TwoErrors   | True    | 19.630 ns | 0.0094 ns | 0.0088 ns |  3.56 |         - |          NA |
| Typed_ThreeErrors | True    | 21.215 ns | 0.0073 ns | 0.0068 ns |  3.85 |         - |          NA |
