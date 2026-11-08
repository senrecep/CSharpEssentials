```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Entries | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |    **72.79 ns** | **0.740 ns** | **0.693 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| HandWritten      | 1       |    54.91 ns | 0.296 ns | 0.262 ns |  0.75 | 0.0091 |     152 B |        1.00 |
|                  |         |             |          |          |       |        |           |             |
| **CSharpEssentials** | **8**       |   **427.75 ns** | **2.912 ns** | **2.724 ns** |  **1.00** | **0.0463** |     **776 B** |        **1.00** |
| HandWritten      | 8       |   305.53 ns | 3.014 ns | 2.672 ns |  0.71 | 0.0463 |     776 B |        1.00 |
|                  |         |             |          |          |       |        |           |             |
| **CSharpEssentials** | **32**      | **1,534.01 ns** | **7.120 ns** | **6.660 ns** |  **1.00** | **0.1431** |    **2400 B** |        **1.00** |
| HandWritten      | 32      |   822.67 ns | 8.193 ns | 7.664 ns |  0.54 | 0.1431 |    2400 B |        1.00 |
