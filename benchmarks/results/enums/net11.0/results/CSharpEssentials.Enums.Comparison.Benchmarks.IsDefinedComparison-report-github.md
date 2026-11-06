```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Defined | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials** | **False**   | **0.4298 ns** | **0.0013 ns** | **0.0012 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Bcl              | False   | 3.5341 ns | 0.0022 ns | 0.0017 ns |  8.22 |    0.02 |         - |          NA |
| EnumsNet         | False   | 1.2765 ns | 0.0089 ns | 0.0083 ns |  2.97 |    0.02 |         - |          NA |
| NetEscapades     | False   | 1.0132 ns | 0.0166 ns | 0.0155 ns |  2.36 |    0.04 |         - |          NA |
|                  |         |           |           |           |       |         |           |             |
| **CSharpEssentials** | **True**    | **0.3781 ns** | **0.0128 ns** | **0.0120 ns** |  **1.00** |    **0.04** |         **-** |          **NA** |
| Bcl              | True    | 3.8279 ns | 0.0022 ns | 0.0018 ns | 10.13 |    0.31 |         - |          NA |
| EnumsNet         | True    | 0.6885 ns | 0.0118 ns | 0.0110 ns |  1.82 |    0.06 |         - |          NA |
| NetEscapades     | True    | 0.4266 ns | 0.0028 ns | 0.0024 ns |  1.13 |    0.04 |         - |          NA |
