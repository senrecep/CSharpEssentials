```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                        | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**                        | **False**   | **1.3167 ns** | **0.0012 ns** | **0.0010 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | False   | 1.5300 ns | 0.0105 ns | 0.0093 ns |  1.16 |         - |          NA |
| Typed_TwoErrors               | False   | 1.9851 ns | 0.0065 ns | 0.0061 ns |  1.51 |         - |          NA |
| Typed_ThreeErrors             | False   | 1.9525 ns | 0.0028 ns | 0.0024 ns |  1.48 |         - |          NA |
| Typed_ThreeErrors_Switch      | False   | 1.3756 ns | 0.0082 ns | 0.0077 ns |  1.04 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | False   | 0.2921 ns | 0.0028 ns | 0.0024 ns |  0.22 |         - |          NA |
|                               |         |           |           |           |       |           |             |
| **Result**                        | **True**    | **1.2065 ns** | **0.0059 ns** | **0.0052 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError                | True    | 1.2255 ns | 0.0088 ns | 0.0082 ns |  1.02 |         - |          NA |
| Typed_TwoErrors               | True    | 1.3086 ns | 0.0030 ns | 0.0028 ns |  1.08 |         - |          NA |
| Typed_ThreeErrors             | True    | 1.6892 ns | 0.0076 ns | 0.0071 ns |  1.40 |         - |          NA |
| Typed_ThreeErrors_Switch      | True    | 0.8346 ns | 0.0073 ns | 0.0068 ns |  0.69 |         - |          NA |
| Typed_ThreeErrors_TryGetValue | True    | 0.5057 ns | 0.0010 ns | 0.0008 ns |  0.42 |         - |          NA |
