```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **245.1 ns** | **2.86 ns** | **2.67 ns** |  **1.00** |    **0.01** |      **-** |      **-** |       **3 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 770.8 ns | 4.48 ns | 4.19 ns |  3.15 |    0.04 | 0.0973 | 0.0820 |    1491 B |      497.00 |
|                                                  |         |          |         |         |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **267.9 ns** | **1.36 ns** | **1.27 ns** |  **1.00** |    **0.01** |      **-** |      **-** |       **5 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 777.0 ns | 5.89 ns | 5.22 ns |  2.90 |    0.02 | 0.0973 | 0.0801 |    1493 B |      298.60 |
