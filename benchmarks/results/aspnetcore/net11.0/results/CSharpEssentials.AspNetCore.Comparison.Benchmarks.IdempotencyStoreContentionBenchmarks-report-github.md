```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **255.0 ns** |  **2.48 ns** |  **2.32 ns** | **254.1 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **59 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 779.0 ns |  6.83 ns |  6.39 ns | 778.9 ns |  3.06 |    0.04 | 0.0973 | 0.0763 |    1483 B |       25.14 |
|                                                  |         |          |          |          |          |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **325.8 ns** |  **6.51 ns** | **15.23 ns** | **332.9 ns** |  **1.00** |    **0.07** | **0.0038** |      **-** |      **63 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 817.1 ns | 15.60 ns | 15.32 ns | 814.8 ns |  2.51 |    0.13 | 0.0973 | 0.0687 |    1487 B |       23.60 |
