```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Entries | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials** | **1**       |   **103.11 ns** | **0.322 ns** | **0.285 ns** |  **1.00** | **0.0148** |     **248 B** |        **1.00** |
| HandWritten      | 1       |    38.90 ns | 0.171 ns | 0.143 ns |  0.38 | 0.0091 |     152 B |        0.61 |
|                  |         |             |          |          |       |        |           |             |
| **CSharpEssentials** | **8**       |   **568.45 ns** | **2.796 ns** | **2.616 ns** |  **1.00** | **0.0830** |    **1392 B** |        **1.00** |
| HandWritten      | 8       |   254.11 ns | 1.678 ns | 1.569 ns |  0.45 | 0.0463 |     776 B |        0.56 |
|                  |         |             |          |          |       |        |           |             |
| **CSharpEssentials** | **32**      | **1,851.62 ns** | **3.189 ns** | **2.663 ns** |  **1.00** | **0.2899** |    **4880 B** |        **1.00** |
| HandWritten      | 32      |   733.09 ns | 4.643 ns | 4.343 ns |  0.40 | 0.1431 |    2400 B |        0.49 |
