```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **0.7609 ns** | **0.0149 ns** | **0.0132 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Bcl              | False   | 3.5401 ns | 0.0007 ns | 0.0005 ns |  4.65 |    0.08 |         - |          NA |
| EnumsNet         | False   | 1.3005 ns | 0.0141 ns | 0.0125 ns |  1.71 |    0.03 |         - |          NA |
| NetEscapades     | False   | 0.7258 ns | 0.0004 ns | 0.0003 ns |  0.95 |    0.02 |         - |          NA |
|                  |         |           |           |           |       |         |           |             |
| **CSharpEssentials** | **True**    | **0.4480 ns** | **0.0057 ns** | **0.0053 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Bcl              | True    | 4.1192 ns | 0.0029 ns | 0.0026 ns |  9.20 |    0.11 |         - |          NA |
| EnumsNet         | True    | 0.7049 ns | 0.0198 ns | 0.0185 ns |  1.57 |    0.04 |         - |          NA |
| NetEscapades     | True    | 0.4532 ns | 0.0074 ns | 0.0069 ns |  1.01 |    0.02 |         - |          NA |
