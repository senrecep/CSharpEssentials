```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **Result**            | **False**   | **15.530 ns** | **0.0998 ns** | **0.0934 ns** |  **1.00** | **0.0033** |      **56 B** |        **1.00** |
| Typed_OneError    | False   |  3.458 ns | 0.0165 ns | 0.0147 ns |  0.22 |      - |         - |        0.00 |
| Typed_TwoErrors   | False   |  8.023 ns | 0.0074 ns | 0.0062 ns |  0.52 |      - |         - |        0.00 |
| Typed_ThreeErrors | False   | 14.275 ns | 0.0319 ns | 0.0283 ns |  0.92 |      - |         - |        0.00 |
|                   |         |           |           |           |       |        |           |             |
| **Result**            | **True**    |  **1.970 ns** | **0.0022 ns** | **0.0019 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Typed_OneError    | True    |  2.230 ns | 0.0036 ns | 0.0033 ns |  1.13 |      - |         - |          NA |
| Typed_TwoErrors   | True    |  7.692 ns | 0.0130 ns | 0.0108 ns |  3.90 |      - |         - |          NA |
| Typed_ThreeErrors | True    | 14.325 ns | 0.0066 ns | 0.0062 ns |  7.27 |      - |         - |          NA |
