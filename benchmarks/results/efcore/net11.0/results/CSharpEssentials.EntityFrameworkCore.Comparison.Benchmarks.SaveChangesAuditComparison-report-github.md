```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                               | Count | State    | Mean         | Error      | StdDev       | Median       | Ratio | RatioSD | Gen0     | Gen1     | Allocated   | Alloc Ratio |
|------------------------------------- |------ |--------- |-------------:|-----------:|-------------:|-------------:|------:|--------:|---------:|---------:|------------:|------------:|
| **Ef_SaveChanges**                       | **1**     | **Modified** |     **54.00 μs** |   **0.215 μs** |     **0.201 μs** |     **53.94 μs** |  **1.00** |    **0.01** |   **0.7324** |   **0.1221** |    **12.21 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Modified |     54.97 μs |   0.743 μs |     0.763 μs |     54.85 μs |  1.02 |    0.01 |   0.4883 |        - |    13.49 KB |        1.10 |
| CSharpEssentials_AuditInterceptor    | 1     | Modified |     58.32 μs |   0.324 μs |     0.271 μs |     58.24 μs |  1.08 |    0.01 |   0.4883 |        - |    13.96 KB |        1.14 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Modified |     59.77 μs |   0.204 μs |     0.160 μs |     59.75 μs |  1.11 |    0.00 |   0.4883 |        - |    13.96 KB |        1.14 |
|                                      |       |          |              |            |              |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1**     | **Added**    |     **61.50 μs** |   **0.252 μs** |     **0.211 μs** |     **61.46 μs** |  **1.00** |    **0.00** |   **0.4883** |        **-** |    **15.91 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Added    |     65.29 μs |   0.452 μs |     0.378 μs |     65.15 μs |  1.06 |    0.01 |   0.9766 |        - |    16.58 KB |        1.04 |
| CSharpEssentials_AuditInterceptor    | 1     | Added    |     67.81 μs |   0.301 μs |     0.235 μs |     67.73 μs |  1.10 |    0.01 |   0.9766 |        - |    17.05 KB |        1.07 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Added    |     65.95 μs |   0.520 μs |     0.434 μs |     65.79 μs |  1.07 |    0.01 |   0.9766 |        - |    17.05 KB |        1.07 |
|                                      |       |          |              |            |              |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Modified** |  **1,897.03 μs** |  **11.738 μs** |     **9.801 μs** |  **1,896.89 μs** |  **1.00** |    **0.01** |  **31.2500** |        **-** |   **725.22 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Modified |  2,087.16 μs |   4.296 μs |     3.354 μs |  2,087.50 μs |  1.10 |    0.01 |  46.8750 |  15.6250 |   842.53 KB |        1.16 |
| CSharpEssentials_AuditInterceptor    | 100   | Modified |  2,101.23 μs |  18.264 μs |    15.251 μs |  2,100.55 μs |  1.11 |    0.01 |  46.8750 |  15.6250 |   843.76 KB |        1.16 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Modified |  2,145.50 μs |   8.432 μs |     7.041 μs |  2,144.74 μs |  1.13 |    0.01 |  46.8750 |  15.6250 |   843.84 KB |        1.16 |
|                                      |       |          |              |            |              |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Added**    |  **2,548.25 μs** |  **12.037 μs** |    **10.671 μs** |  **2,546.68 μs** |  **1.00** |    **0.01** |  **62.5000** |        **-** |  **1090.94 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Added    |  2,648.24 μs |  19.752 μs |    17.509 μs |  2,650.84 μs |  1.04 |    0.01 |  62.5000 |        - |   1143.4 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 100   | Added    |  2,631.28 μs |  14.540 μs |    12.889 μs |  2,628.86 μs |  1.03 |    0.01 |  62.5000 |        - |  1144.65 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Added    |  2,593.32 μs |  10.278 μs |     8.582 μs |  2,590.80 μs |  1.02 |    0.01 |  62.5000 |        - |  1144.72 KB |        1.05 |
|                                      |       |          |              |            |              |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Modified** | **18,511.63 μs** |  **72.161 μs** |    **67.500 μs** | **18,489.91 μs** |  **1.00** |    **0.00** | **437.5000** | **218.7500** |  **7191.94 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Modified | 21,340.98 μs | 379.351 μs |   354.845 μs | 21,242.02 μs |  1.15 |    0.02 | 500.0000 | 250.0000 |  8364.11 KB |        1.16 |
| CSharpEssentials_AuditInterceptor    | 1000  | Modified | 22,043.16 μs | 434.044 μs |   594.125 μs | 21,839.02 μs |  1.19 |    0.03 | 500.0000 | 250.0000 |  8372.38 KB |        1.16 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Modified | 22,131.50 μs | 436.128 μs |   678.999 μs | 21,782.87 μs |  1.20 |    0.04 | 500.0000 | 250.0000 |  8372.29 KB |        1.16 |
|                                      |       |          |              |            |              |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Added**    | **24,474.74 μs** | **483.773 μs** |   **738.772 μs** | **24,075.63 μs** |  **1.00** |    **0.04** | **333.3333** |        **-** | **10848.49 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Added    | 27,050.79 μs | 448.695 μs |   614.179 μs | 26,871.46 μs |  1.11 |    0.04 | 666.6667 | 333.3333 | 11371.76 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 1000  | Added    | 27,032.64 μs | 527.010 μs |   703.543 μs | 26,650.04 μs |  1.11 |    0.04 | 666.6667 | 333.3333 | 11380.25 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Added    | 27,309.38 μs | 541.941 μs | 1,017.897 μs | 26,739.79 μs |  1.12 |    0.05 | 666.6667 | 333.3333 | 11380.42 KB |        1.05 |
