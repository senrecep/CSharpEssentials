```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|--------:|--------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **261.6 ns** | **1.97 ns** | **1.84 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **59 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 730.5 ns | 5.64 ns | 5.00 ns |  2.79 |    0.03 | 0.0973 | 0.0820 |    1483 B |       25.14 |
|                                                  |         |          |         |         |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **283.3 ns** | **2.43 ns** | **2.16 ns** |  **1.00** |    **0.01** | **0.0033** |      **-** |      **61 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 775.3 ns | 3.91 ns | 3.47 ns |  2.74 |    0.02 | 0.0982 | 0.0792 |    1485 B |       24.34 |
