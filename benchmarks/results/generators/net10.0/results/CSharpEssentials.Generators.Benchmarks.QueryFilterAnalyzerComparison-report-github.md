```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | Files | Mean       | Error      | StdDev     | Median     | Ratio | RatioSD | Gen0       | Gen1      | Gen2      | Allocated    | Alloc Ratio |
|----------------------------- |------------ |------ |-----------:|-----------:|-----------:|-----------:|------:|--------:|-----------:|----------:|----------:|-------------:|------------:|
| **NoOpAnalyzer_Compilation**     | **Compilation** | **200**   | **103.280 ms** |  **1.9557 ms** |  **4.9065 ms** | **101.999 ms** |  **0.74** |    **0.11** |  **2000.0000** | **1000.0000** |         **-** |  **38523.54 KB** |        **0.92** |
| CSharpEssentials_Compilation | Compilation | 200   | 142.563 ms |  7.9753 ms | 23.2642 ms | 133.223 ms |  1.02 |    0.22 |  2000.0000 | 1000.0000 |         - |  41877.07 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Compilation**     | **Compilation** | **1000**  | **613.849 ms** | **11.6862 ms** | **13.9116 ms** | **612.890 ms** |  **1.00** |    **0.05** | **13000.0000** | **8000.0000** | **1000.0000** | **200841.48 KB** |        **0.95** |
| CSharpEssentials_Compilation | Compilation | 1000  | 616.659 ms | 12.2718 ms | 27.9491 ms | 612.761 ms |  1.00 |    0.06 | 12000.0000 | 6000.0000 |         - | 210655.31 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Document**        | **Document**    | **200**   |   **2.623 ms** |  **0.0449 ms** |  **0.0699 ms** |   **2.603 ms** |  **0.76** |    **0.11** |    **46.8750** |   **15.6250** |         **-** |    **769.05 KB** |        **0.91** |
| CSharpEssentials_Document    | Document    | 200   |   3.506 ms |  0.1988 ms |  0.5768 ms |   3.213 ms |  1.02 |    0.22 |    31.2500 |         - |         - |    844.17 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Document**        | **Document**    | **1000**  |   **8.350 ms** |  **0.1338 ms** |  **0.2641 ms** |   **8.278 ms** |  **0.91** |    **0.04** |   **125.0000** |   **93.7500** |         **-** |   **2653.01 KB** |        **0.97** |
| CSharpEssentials_Document    | Document    | 1000  |   9.176 ms |  0.1693 ms |  0.2317 ms |   9.113 ms |  1.00 |    0.03 |   156.2500 |  125.0000 |         - |   2727.68 KB |        1.00 |
