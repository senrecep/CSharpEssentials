```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **242.3 ns** |  **1.15 ns** |  **1.08 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **59 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 863.7 ns |  8.72 ns |  7.73 ns |  3.56 |    0.03 | 0.0973 | 0.0820 |    1483 B |       25.14 |
|                                                  |         |          |          |          |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **296.6 ns** |  **2.95 ns** |  **2.76 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **61 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 853.9 ns | 13.77 ns | 12.21 ns |  2.88 |    0.05 | 0.0973 | 0.0820 |    1485 B |       24.34 |
