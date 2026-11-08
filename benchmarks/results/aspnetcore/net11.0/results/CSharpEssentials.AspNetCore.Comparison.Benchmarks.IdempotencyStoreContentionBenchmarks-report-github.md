```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|--------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **247.3 ns** | **0.97 ns** |  **0.91 ns** |  **1.00** |    **0.01** |      **-** |      **-** |       **3 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 688.1 ns | 6.64 ns |  6.21 ns |  2.78 |    0.03 | 0.0973 | 0.0820 |    1491 B |      497.00 |
|                                                  |         |          |         |          |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **295.8 ns** | **5.92 ns** | **12.09 ns** |  **1.00** |    **0.06** |      **-** |      **-** |       **6 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 706.1 ns | 9.54 ns |  8.92 ns |  2.39 |    0.11 | 0.0973 | 0.0801 |    1493 B |      248.83 |
