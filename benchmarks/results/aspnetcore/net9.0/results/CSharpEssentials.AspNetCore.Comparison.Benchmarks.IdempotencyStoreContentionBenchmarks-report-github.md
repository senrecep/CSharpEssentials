```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **248.4 ns** |  **1.40 ns** |  **1.31 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **59 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 849.2 ns |  6.63 ns |  5.54 ns |  3.42 |    0.03 | 0.0973 | 0.0820 |    1483 B |       25.14 |
|                                                  |         |          |          |          |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **278.4 ns** |  **1.91 ns** |  **1.69 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **61 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 863.7 ns | 17.01 ns | 19.58 ns |  3.10 |    0.07 | 0.0973 | 0.0820 |    1485 B |       24.34 |
