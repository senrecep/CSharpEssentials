```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Entries | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |   **110.68 ns** |  **0.255 ns** |  **0.239 ns** |  **1.00** | **0.0148** |     **248 B** |        **1.00** |
| HandWritten      | 1       |    48.34 ns |  0.226 ns |  0.200 ns |  0.44 | 0.0091 |     152 B |        0.61 |
|                  |         |             |           |           |       |        |           |             |
| **CSharpEssentials** | **8**       |   **544.72 ns** |  **3.557 ns** |  **3.328 ns** |  **1.00** | **0.0830** |    **1392 B** |        **1.00** |
| HandWritten      | 8       |   265.82 ns |  1.568 ns |  1.467 ns |  0.49 | 0.0463 |     776 B |        0.56 |
|                  |         |             |           |           |       |        |           |             |
| **CSharpEssentials** | **32**      | **1,914.28 ns** | **11.226 ns** | **10.501 ns** |  **1.00** | **0.2899** |    **4880 B** |        **1.00** |
| HandWritten      | 32      |   725.93 ns |  4.775 ns |  4.466 ns |  0.38 | 0.1431 |    2400 B |        0.49 |
