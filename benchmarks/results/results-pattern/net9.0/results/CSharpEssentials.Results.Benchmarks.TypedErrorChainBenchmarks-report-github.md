```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Result**            | **False**   | **17.602 ns** | **0.1089 ns** | **0.0966 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| Typed_OneError    | False   |  9.214 ns | 0.0030 ns | 0.0023 ns |  0.52 |    0.00 |      - |         - |        0.00 |
| Typed_TwoErrors   | False   | 27.515 ns | 0.0257 ns | 0.0215 ns |  1.56 |    0.01 |      - |         - |        0.00 |
| Typed_ThreeErrors | False   | 36.233 ns | 0.0423 ns | 0.0375 ns |  2.06 |    0.01 |      - |         - |        0.00 |
|                   |         |           |           |           |       |         |        |           |             |
| **Result**            | **True**    |  **2.888 ns** | **0.0049 ns** | **0.0041 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Typed_OneError    | True    |  8.646 ns | 0.0129 ns | 0.0121 ns |  2.99 |    0.01 |      - |         - |          NA |
| Typed_TwoErrors   | True    | 28.204 ns | 0.0300 ns | 0.0266 ns |  9.77 |    0.02 |      - |         - |          NA |
| Typed_ThreeErrors | True    | 38.186 ns | 0.0171 ns | 0.0151 ns | 13.22 |    0.02 |      - |         - |          NA |
