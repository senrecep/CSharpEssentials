```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Entries | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |   **102.66 ns** |  **1.805 ns** |  **1.688 ns** |  **1.00** |    **0.02** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten      | 1       |    56.75 ns |  0.532 ns |  0.497 ns |  0.55 |    0.01 | 0.0091 |     152 B |        1.00 |
|                  |         |             |           |           |       |         |        |           |             |
| **CSharpEssentials** | **8**       |   **693.31 ns** |  **7.034 ns** |  **6.579 ns** |  **1.00** |    **0.01** | **0.0458** |     **776 B** |        **1.00** |
| HandWritten      | 8       |   299.96 ns |  3.181 ns |  2.976 ns |  0.43 |    0.01 | 0.0463 |     776 B |        1.00 |
|                  |         |             |           |           |       |         |        |           |             |
| **CSharpEssentials** | **32**      | **2,561.28 ns** | **25.475 ns** | **23.830 ns** |  **1.00** |    **0.01** | **0.1411** |    **2400 B** |        **1.00** |
| HandWritten      | 32      |   827.38 ns |  9.258 ns |  8.660 ns |  0.32 |    0.00 | 0.1431 |    2400 B |        1.00 |
