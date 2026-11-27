```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **Result**            | **False**   | **41.027 ns** | **0.0612 ns** | **0.0542 ns** |  **1.00** | **0.0033** |      **56 B** |        **1.00** |
| Typed_OneError    | False   |  9.210 ns | 0.0061 ns | 0.0057 ns |  0.22 |      - |         - |        0.00 |
| Typed_TwoErrors   | False   | 29.551 ns | 0.0114 ns | 0.0101 ns |  0.72 |      - |         - |        0.00 |
| Typed_ThreeErrors | False   | 38.609 ns | 0.0218 ns | 0.0193 ns |  0.94 |      - |         - |        0.00 |
|                   |         |           |           |           |       |        |           |             |
| **Result**            | **True**    |  **3.571 ns** | **0.0035 ns** | **0.0029 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Typed_OneError    | True    |  8.932 ns | 0.0038 ns | 0.0032 ns |  2.50 |      - |         - |          NA |
| Typed_TwoErrors   | True    | 25.715 ns | 0.0157 ns | 0.0147 ns |  7.20 |      - |         - |          NA |
| Typed_ThreeErrors | True    | 36.427 ns | 0.0238 ns | 0.0223 ns | 10.20 |      - |         - |          NA |
