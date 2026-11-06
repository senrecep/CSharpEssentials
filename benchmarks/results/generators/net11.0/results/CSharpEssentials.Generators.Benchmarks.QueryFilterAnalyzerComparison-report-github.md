```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | Files | Mean       | Error      | StdDev     | Median     | Ratio | RatioSD | Gen0       | Gen1      | Gen2      | Allocated    | Alloc Ratio |
|----------------------------- |------------ |------ |-----------:|-----------:|-----------:|-----------:|------:|--------:|-----------:|----------:|----------:|-------------:|------------:|
| **NoOpAnalyzer_Compilation**     | **Compilation** | **200**   | **123.566 ms** | **10.1651 ms** | **29.8124 ms** | **113.278 ms** |  **0.90** |    **0.25** |  **2000.0000** | **1000.0000** |         **-** |  **38262.87 KB** |        **0.91** |
| CSharpEssentials_Compilation | Compilation | 200   | 140.999 ms |  7.8964 ms | 22.6562 ms | 131.944 ms |  1.02 |    0.22 |  2000.0000 | 1000.0000 |         - |  41951.53 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Compilation**     | **Compilation** | **1000**  | **599.239 ms** | **11.9823 ms** | **17.9346 ms** | **601.345 ms** |  **0.97** |    **0.06** | **12000.0000** | **7000.0000** |         **-** | **200074.02 KB** |        **0.96** |
| CSharpEssentials_Compilation | Compilation | 1000  | 616.412 ms | 12.3223 ms | 31.3642 ms | 614.177 ms |  1.00 |    0.07 | 13000.0000 | 7000.0000 | 1000.0000 | 209297.73 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Document**        | **Document**    | **200**   |   **2.531 ms** |  **0.0461 ms** |  **0.0757 ms** |   **2.512 ms** |  **0.78** |    **0.10** |    **46.8750** |   **15.6250** |         **-** |    **765.01 KB** |        **0.91** |
| CSharpEssentials_Document    | Document    | 200   |   3.298 ms |  0.1704 ms |  0.4833 ms |   3.038 ms |  1.02 |    0.19 |    31.2500 |         - |         - |    839.28 KB |        1.00 |
|                              |             |       |            |            |            |            |       |         |            |           |           |              |             |
| **NoOpAnalyzer_Document**        | **Document**    | **1000**  |   **8.331 ms** |  **0.1593 ms** |  **0.2385 ms** |   **8.275 ms** |  **0.90** |    **0.03** |   **125.0000** |   **93.7500** |         **-** |   **2654.48 KB** |        **0.97** |
| CSharpEssentials_Document    | Document    | 1000  |   9.218 ms |  0.1789 ms |  0.2508 ms |   9.137 ms |  1.00 |    0.04 |   156.2500 |  125.0000 |         - |   2727.06 KB |        1.00 |
