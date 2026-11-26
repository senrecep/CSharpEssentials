```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   |  **4.308 ns** | **0.0030 ns** | **0.0025 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   |  9.215 ns | 0.0106 ns | 0.0099 ns |  2.14 |         - |          NA |
| Typed_TwoErrors   | False   |  9.488 ns | 0.0086 ns | 0.0080 ns |  2.20 |         - |          NA |
| Typed_ThreeErrors | False   |  9.205 ns | 0.0144 ns | 0.0135 ns |  2.14 |         - |          NA |
|                   |         |           |           |           |       |           |             |
| **Result**            | **True**    |  **4.989 ns** | **0.0078 ns** | **0.0069 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 18.722 ns | 0.0183 ns | 0.0171 ns |  3.75 |         - |          NA |
| Typed_TwoErrors   | True    | 19.100 ns | 0.0135 ns | 0.0126 ns |  3.83 |         - |          NA |
| Typed_ThreeErrors | True    | 17.173 ns | 0.0125 ns | 0.0117 ns |  3.44 |         - |          NA |
