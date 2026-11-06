```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Entries | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |    **97.40 ns** | **0.316 ns** | **0.296 ns** |  **1.00** | **0.0148** |     **248 B** |        **1.00** |
| HandWritten      | 1       |    42.31 ns | 0.227 ns | 0.213 ns |  0.43 | 0.0091 |     152 B |        0.61 |
|                  |         |             |          |          |       |        |           |             |
| **CSharpEssentials** | **8**       |   **525.02 ns** | **4.461 ns** | **4.172 ns** |  **1.00** | **0.0830** |    **1392 B** |        **1.00** |
| HandWritten      | 8       |   258.59 ns | 1.737 ns | 1.625 ns |  0.49 | 0.0463 |     776 B |        0.56 |
|                  |         |             |          |          |       |        |           |             |
| **CSharpEssentials** | **32**      | **1,723.50 ns** | **6.269 ns** | **5.557 ns** |  **1.00** | **0.2899** |    **4880 B** |        **1.00** |
| HandWritten      | 32      |   736.66 ns | 5.827 ns | 5.451 ns |  0.43 | 0.1431 |    2400 B |        0.49 |
