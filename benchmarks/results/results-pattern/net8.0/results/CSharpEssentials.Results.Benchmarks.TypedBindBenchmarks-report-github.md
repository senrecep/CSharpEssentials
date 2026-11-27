```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   |  **7.183 ns** | **0.0098 ns** | **0.0087 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   |  9.520 ns | 0.0098 ns | 0.0086 ns |  1.33 |         - |          NA |
| Typed_TwoErrors   | False   |  9.564 ns | 0.0092 ns | 0.0086 ns |  1.33 |         - |          NA |
| Typed_ThreeErrors | False   |  9.565 ns | 0.0081 ns | 0.0072 ns |  1.33 |         - |          NA |
|                   |         |           |           |           |       |           |             |
| **Result**            | **True**    |  **5.791 ns** | **0.0083 ns** | **0.0069 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 19.256 ns | 0.0182 ns | 0.0170 ns |  3.33 |         - |          NA |
| Typed_TwoErrors   | True    | 19.534 ns | 0.0439 ns | 0.0366 ns |  3.37 |         - |          NA |
| Typed_ThreeErrors | True    | 21.220 ns | 0.0079 ns | 0.0074 ns |  3.66 |         - |          NA |
