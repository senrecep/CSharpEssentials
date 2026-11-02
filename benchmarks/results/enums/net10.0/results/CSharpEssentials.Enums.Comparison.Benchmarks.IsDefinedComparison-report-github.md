```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **2.8831 ns** | **0.0069 ns** | **0.0065 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | False   | 3.8319 ns | 0.0012 ns | 0.0010 ns |  1.33 |         - |          NA |
| EnumsNet         | False   | 1.2988 ns | 0.0100 ns | 0.0094 ns |  0.45 |         - |          NA |
| NetEscapades     | False   | 0.7315 ns | 0.0005 ns | 0.0004 ns |  0.25 |         - |          NA |
|                  |         |           |           |           |       |           |             |
| **CSharpEssentials** | **True**    | **3.9082 ns** | **0.0071 ns** | **0.0063 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | True    | 4.4041 ns | 0.0030 ns | 0.0026 ns |  1.13 |         - |          NA |
| EnumsNet         | True    | 0.8611 ns | 0.0134 ns | 0.0126 ns |  0.22 |         - |          NA |
| NetEscapades     | True    | 0.4209 ns | 0.0044 ns | 0.0037 ns |  0.11 |         - |          NA |
