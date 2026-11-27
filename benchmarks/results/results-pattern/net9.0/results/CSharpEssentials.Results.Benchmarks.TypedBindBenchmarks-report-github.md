```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **Result**            | **False**   |  **4.364 ns** | **0.0094 ns** | **0.0083 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | False   |  9.227 ns | 0.0118 ns | 0.0104 ns |  2.11 |         - |          NA |
| Typed_TwoErrors   | False   |  9.486 ns | 0.0060 ns | 0.0056 ns |  2.17 |         - |          NA |
| Typed_ThreeErrors | False   |  9.489 ns | 0.0036 ns | 0.0032 ns |  2.17 |         - |          NA |
|                   |         |           |           |           |       |           |             |
| **Result**            | **True**    |  **4.881 ns** | **0.0133 ns** | **0.0125 ns** |  **1.00** |         **-** |          **NA** |
| Typed_OneError    | True    | 16.803 ns | 0.0079 ns | 0.0066 ns |  3.44 |         - |          NA |
| Typed_TwoErrors   | True    | 19.082 ns | 0.0086 ns | 0.0077 ns |  3.91 |         - |          NA |
| Typed_ThreeErrors | True    | 16.852 ns | 0.0098 ns | 0.0082 ns |  3.45 |         - |          NA |
