```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Result**            | **False**   | **16.023 ns** | **0.0291 ns** | **0.0243 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Typed_OneError    | False   |  3.177 ns | 0.0048 ns | 0.0042 ns |  0.20 |    0.00 |      - |         - |        0.00 |
| Typed_TwoErrors   | False   |  7.734 ns | 0.0049 ns | 0.0041 ns |  0.48 |    0.00 |      - |         - |        0.00 |
| Typed_ThreeErrors | False   | 14.178 ns | 0.0362 ns | 0.0339 ns |  0.88 |    0.00 |      - |         - |        0.00 |
|                   |         |           |           |           |       |         |        |           |             |
| **Result**            | **True**    |  **2.647 ns** | **0.0090 ns** | **0.0075 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Typed_OneError    | True    |  3.143 ns | 0.0038 ns | 0.0030 ns |  1.19 |    0.00 |      - |         - |          NA |
| Typed_TwoErrors   | True    |  8.196 ns | 0.0093 ns | 0.0087 ns |  3.10 |    0.01 |      - |         - |          NA |
| Typed_ThreeErrors | True    | 13.989 ns | 0.0534 ns | 0.0500 ns |  5.29 |    0.02 |      - |         - |          NA |
