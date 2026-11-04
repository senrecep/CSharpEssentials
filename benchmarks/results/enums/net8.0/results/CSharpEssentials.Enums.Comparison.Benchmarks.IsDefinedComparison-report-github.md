```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **7.8643 ns** | **0.0079 ns** | **0.0074 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | False   | 5.0596 ns | 0.0205 ns | 0.0191 ns |  0.64 |         - |          NA |
| EnumsNet         | False   | 1.3184 ns | 0.0044 ns | 0.0041 ns |  0.17 |         - |          NA |
| NetEscapades     | False   | 0.4271 ns | 0.0033 ns | 0.0030 ns |  0.05 |         - |          NA |
|                  |         |           |           |           |       |           |             |
| **CSharpEssentials** | **True**    | **8.1692 ns** | **0.0104 ns** | **0.0097 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | True    | 5.0849 ns | 0.0194 ns | 0.0182 ns |  0.62 |         - |          NA |
| EnumsNet         | True    | 0.6977 ns | 0.0175 ns | 0.0155 ns |  0.09 |         - |          NA |
| NetEscapades     | True    | 0.4639 ns | 0.0100 ns | 0.0094 ns |  0.06 |         - |          NA |
