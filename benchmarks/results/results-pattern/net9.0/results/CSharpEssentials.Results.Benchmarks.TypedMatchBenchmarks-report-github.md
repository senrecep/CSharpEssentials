```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **Result**                        | **False**   | **1.3190 ns** | **0.0020 ns** | **0.0018 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.4510 ns | 0.0042 ns | 0.0039 ns |  1.10 |    0.00 |         - |          NA |
| Typed_TwoErrors               | False   | 1.6355 ns | 0.0021 ns | 0.0018 ns |  1.24 |    0.00 |         - |          NA |
| Typed_ThreeErrors             | False   | 2.2001 ns | 0.0021 ns | 0.0020 ns |  1.67 |    0.00 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3388 ns | 0.0048 ns | 0.0043 ns |  1.02 |    0.00 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.7445 ns | 0.0043 ns | 0.0040 ns |  0.56 |    0.00 |         - |          NA |
|                               |         |           |           |           |       |         |           |             |
| **Result**                        | **True**    | **1.2372 ns** | **0.0107 ns** | **0.0100 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2279 ns | 0.0093 ns | 0.0087 ns |  0.99 |    0.01 |         - |          NA |
| Typed_TwoErrors               | True    | 1.5890 ns | 0.0046 ns | 0.0043 ns |  1.28 |    0.01 |         - |          NA |
| Typed_ThreeErrors             | True    | 2.0583 ns | 0.0114 ns | 0.0101 ns |  1.66 |    0.02 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.8104 ns | 0.0045 ns | 0.0042 ns |  0.66 |    0.01 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.4696 ns | 0.0011 ns | 0.0010 ns |  0.38 |    0.00 |         - |          NA |
