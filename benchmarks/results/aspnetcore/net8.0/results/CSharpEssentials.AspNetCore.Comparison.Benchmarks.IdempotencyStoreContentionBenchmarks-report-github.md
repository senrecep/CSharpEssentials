```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2

Categories=Contended  

```
| Method                                           | Threads | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------------------- |-------- |---------:|---------:|--------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_InMemory_Contended_SameKey**      | **8**       | **256.4 ns** |  **0.70 ns** | **0.65 ns** |  **1.00** |    **0.00** |      **-** |      **-** |       **3 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 8       | 762.1 ns |  5.63 ns | 5.27 ns |  2.97 |    0.02 | 0.0973 | 0.0782 |    1491 B |      497.00 |
|                                                  |         |          |          |         |       |         |        |        |           |             |
| **CSharpEssentials_InMemory_Contended_SameKey**      | **32**      | **266.1 ns** |  **2.27 ns** | **2.01 ns** |  **1.00** |    **0.01** |      **-** |      **-** |       **5 B** |        **1.00** |
| CSharpEssentials_InMemory_Contended_DistinctKeys | 32      | 792.0 ns | 10.19 ns | 9.53 ns |  2.98 |    0.04 | 0.0973 | 0.0782 |    1493 B |      298.60 |
