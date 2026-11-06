```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Categories  | Files | Mean       | Error      | StdDev     | Median     | Ratio | RatioSD | Gen0       | Gen1      | Gen2      | Allocated    | Alloc Ratio |
|----------------------------- |------------ |------ |-----------:|-----------:|-----------:|-----------:|------:|--------:|-----------:|----------:|----------:|-------------:|------------:|
| **NoOpAnalyzer_Compilation**     | **Compilation** | **200**   | **107.353 ms** |  **2.1328 ms** |  **5.1509 ms** | **105.782 ms** |  **0.78** |    **0.07** |  **2000.0000** | **1000.0000** |         **-** |  **37962.37 KB** |        **0.89** |
| CSharpEssentials_Compilation | Compilation | 200   | 138.569 ms |  4.1924 ms | 11.9612 ms | 134.776 ms |  1.01 |    0.12 |  2000.0000 | 1000.0000 |         - |  42822.46 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Compilation**     | **Compilation** | **1000**  | **631.812 ms** | **12.3718 ms** | **22.6226 ms** | **633.792 ms** |  **0.98** |    **0.05** | **12000.0000** | **6000.0000** |         **-** | **207850.09 KB** |        **1.01** |
| CSharpEssentials_Compilation | Compilation | 1000  | 642.773 ms | 12.6634 ms | 24.3981 ms | 642.232 ms |  1.00 |    0.05 | 13000.0000 | 8000.0000 | 1000.0000 | 206207.87 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Document**        | **Document**    | **200**   |   **2.715 ms** |  **0.0358 ms** |  **0.0317 ms** |   **2.720 ms** |  **0.80** |    **0.01** |    **46.8750** |   **15.6250** |         **-** |    **771.93 KB** |        **0.91** |
| CSharpEssentials_Document    | Document    | 200   |   3.404 ms |  0.0310 ms |  0.0274 ms |   3.411 ms |  1.00 |    0.01 |    46.8750 |   15.6250 |         - |    845.57 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Document**        | **Document**    | **1000**  |   **8.414 ms** |  **0.1514 ms** |  **0.1416 ms** |   **8.381 ms** |  **0.89** |    **0.02** |   **125.0000** |   **93.7500** |         **-** |   **2656.42 KB** |        **0.97** |
| CSharpEssentials_Document    | Document    | 1000  |   9.405 ms |  0.1778 ms |  0.1663 ms |   9.391 ms |  1.00 |    0.02 |   156.2500 |  125.0000 |         - |   2729.68 KB |        1.00 |
