```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error   | StdDev  | Median   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|--------:|--------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **242.6 ns** | **4.85 ns** | **5.95 ns** | **238.8 ns** |  **1.00** |    **0.03** |      **-** |      **-** |       **3 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 671.9 ns | 4.32 ns | 4.05 ns | 670.8 ns |  2.77 |    0.07 | 0.0973 | 0.0811 |    1491 B |      497.00 |
|                                                  |         |          |         |         |          |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **263.1 ns** | **2.08 ns** | **1.95 ns** | **263.3 ns** |  **1.00** |    **0.01** |      **-** |      **-** |       **5 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 730.8 ns | 5.56 ns | 5.20 ns | 731.3 ns |  2.78 |    0.03 | 0.0973 | 0.0839 |    1493 B |      298.60 |
