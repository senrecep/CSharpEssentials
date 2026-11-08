```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Entries | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |  **50.80 ns** |  **0.607 ns** |  **0.568 ns** |  **1.00** |    **0.02** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten      | 1       |  45.03 ns |  0.636 ns |  0.595 ns |  0.89 |    0.01 | 0.0091 |     152 B |        1.00 |
|                  |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials** | **8**       | **296.95 ns** |  **2.624 ns** |  **2.191 ns** |  **1.00** |    **0.01** | **0.0463** |     **776 B** |        **1.00** |
| HandWritten      | 8       | 269.67 ns |  3.003 ns |  2.809 ns |  0.91 |    0.01 | 0.0463 |     776 B |        1.00 |
|                  |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials** | **32**      | **885.66 ns** | **11.570 ns** | **10.823 ns** |  **1.00** |    **0.02** | **0.1431** |    **2400 B** |        **1.00** |
| HandWritten      | 32      | 767.05 ns | 14.505 ns | 12.859 ns |  0.87 |    0.02 | 0.1431 |    2400 B |        1.00 |
