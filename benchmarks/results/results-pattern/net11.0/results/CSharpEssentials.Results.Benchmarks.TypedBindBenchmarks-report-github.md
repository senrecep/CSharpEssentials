```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |---------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   | **3.008 ns** | **0.0057 ns** | **0.0053 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   | 2.650 ns | 0.0075 ns | 0.0067 ns |  0.88 |         - |          NA |
| Typed_TwoErrors   | False   | 2.984 ns | 0.0083 ns | 0.0074 ns |  0.99 |         - |          NA |
| Typed_ThreeErrors | False   | 2.892 ns | 0.0124 ns | 0.0103 ns |  0.96 |         - |          NA |
|                   |         |          |           |           |       |           |             |
| **Result**            | **True**    | **2.578 ns** | **0.0024 ns** | **0.0020 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 4.174 ns | 0.0106 ns | 0.0099 ns |  1.62 |         - |          NA |
| Typed_TwoErrors   | True    | 3.977 ns | 0.0024 ns | 0.0020 ns |  1.54 |         - |          NA |
| Typed_ThreeErrors | True    | 4.306 ns | 0.0079 ns | 0.0074 ns |  1.67 |         - |          NA |
