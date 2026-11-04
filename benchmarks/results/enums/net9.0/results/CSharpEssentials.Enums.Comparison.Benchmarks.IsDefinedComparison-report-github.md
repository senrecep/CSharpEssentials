```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **6.7179 ns** | **0.0023 ns** | **0.0018 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | False   | 5.3258 ns | 0.0198 ns | 0.0175 ns |  0.79 |         - |          NA |
| EnumsNet         | False   | 1.3052 ns | 0.0039 ns | 0.0036 ns |  0.19 |         - |          NA |
| NetEscapades     | False   | 0.4432 ns | 0.0065 ns | 0.0061 ns |  0.07 |         - |          NA |
|                  |         |           |           |           |       |           |             |
| **CSharpEssentials** | **True**    | **7.0025 ns** | **0.0031 ns** | **0.0024 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | True    | 5.7329 ns | 0.0107 ns | 0.0095 ns |  0.82 |         - |          NA |
| EnumsNet         | True    | 0.6959 ns | 0.0259 ns | 0.0242 ns |  0.10 |         - |          NA |
| NetEscapades     | True    | 0.7306 ns | 0.0027 ns | 0.0025 ns |  0.10 |         - |          NA |
