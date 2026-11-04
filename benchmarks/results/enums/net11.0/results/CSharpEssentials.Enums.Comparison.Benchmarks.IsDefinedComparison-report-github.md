```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **2.9813 ns** | **0.0084 ns** | **0.0070 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | False   | 3.6894 ns | 0.0052 ns | 0.0046 ns |  1.24 |         - |          NA |
| EnumsNet         | False   | 1.2766 ns | 0.0039 ns | 0.0036 ns |  0.43 |         - |          NA |
| NetEscapades     | False   | 0.7192 ns | 0.0018 ns | 0.0016 ns |  0.24 |         - |          NA |
|                  |         |           |           |           |       |           |             |
| **CSharpEssentials** | **True**    | **3.3945 ns** | **0.0038 ns** | **0.0032 ns** |  **1.00** |         **-** |          **NA** |
| Bcl              | True    | 3.8336 ns | 0.0180 ns | 0.0169 ns |  1.13 |         - |          NA |
| EnumsNet         | True    | 0.6909 ns | 0.0077 ns | 0.0072 ns |  0.20 |         - |          NA |
| NetEscapades     | True    | 0.3851 ns | 0.0259 ns | 0.0216 ns |  0.11 |         - |          NA |
