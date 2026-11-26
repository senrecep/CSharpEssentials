```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **Result**                        | **False**   | **1.3203 ns** | **0.0032 ns** | **0.0028 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.5303 ns | 0.0076 ns | 0.0071 ns |  1.16 |    0.01 |         - |          NA |
| Typed_TwoErrors               | False   | 3.3400 ns | 0.0036 ns | 0.0030 ns |  2.53 |    0.01 |         - |          NA |
| Typed_ThreeErrors             | False   | 4.2252 ns | 0.0019 ns | 0.0016 ns |  3.20 |    0.01 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3389 ns | 0.0054 ns | 0.0050 ns |  1.01 |    0.00 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.3200 ns | 0.0063 ns | 0.0059 ns |  0.24 |    0.00 |         - |          NA |
|                               |         |           |           |           |       |         |           |             |
| **Result**                        | **True**    | **1.2162 ns** | **0.0066 ns** | **0.0058 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2598 ns | 0.0046 ns | 0.0043 ns |  1.04 |    0.01 |         - |          NA |
| Typed_TwoErrors               | True    | 2.6050 ns | 0.0027 ns | 0.0026 ns |  2.14 |    0.01 |         - |          NA |
| Typed_ThreeErrors             | True    | 3.3954 ns | 0.0072 ns | 0.0060 ns |  2.79 |    0.01 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.9409 ns | 0.0240 ns | 0.0225 ns |  0.77 |    0.02 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.5064 ns | 0.0028 ns | 0.0024 ns |  0.42 |    0.00 |         - |          NA |
