```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |---------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   | **2.850 ns** | **0.0032 ns** | **0.0029 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   | 2.643 ns | 0.0065 ns | 0.0061 ns |  0.93 |         - |          NA |
| Typed_TwoErrors   | False   | 3.051 ns | 0.0087 ns | 0.0081 ns |  1.07 |         - |          NA |
| Typed_ThreeErrors | False   | 2.905 ns | 0.0153 ns | 0.0143 ns |  1.02 |         - |          NA |
|                   |         |          |           |           |       |           |             |
| **Result**            | **True**    | **3.394 ns** | **0.0020 ns** | **0.0017 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 4.017 ns | 0.0061 ns | 0.0047 ns |  1.18 |         - |          NA |
| Typed_TwoErrors   | True    | 4.136 ns | 0.0072 ns | 0.0064 ns |  1.22 |         - |          NA |
| Typed_ThreeErrors | True    | 4.023 ns | 0.0094 ns | 0.0088 ns |  1.19 |         - |          NA |
