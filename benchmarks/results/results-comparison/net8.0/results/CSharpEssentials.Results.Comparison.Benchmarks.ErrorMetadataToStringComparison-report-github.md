```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Entries | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |   **167.49 ns** |  **0.492 ns** |  **0.411 ns** |  **1.00** | **0.0191** |     **320 B** |        **1.00** |
| HandWritten      | 1       |    50.93 ns |  0.223 ns |  0.208 ns |  0.30 | 0.0091 |     152 B |        0.47 |
|                  |         |             |           |           |       |        |           |             |
| **CSharpEssentials** | **8**       | **1,128.49 ns** |  **4.922 ns** |  **4.604 ns** |  **1.00** | **0.1163** |    **1968 B** |        **1.00** |
| HandWritten      | 8       |   272.86 ns |  1.366 ns |  1.278 ns |  0.24 | 0.0463 |     776 B |        0.39 |
|                  |         |             |           |           |       |        |           |             |
| **CSharpEssentials** | **32**      | **3,786.76 ns** | **13.678 ns** | **12.795 ns** |  **1.00** | **0.4272** |    **7184 B** |        **1.00** |
| HandWritten      | 32      |   725.81 ns |  3.848 ns |  3.599 ns |  0.19 | 0.1431 |    2400 B |        0.33 |
