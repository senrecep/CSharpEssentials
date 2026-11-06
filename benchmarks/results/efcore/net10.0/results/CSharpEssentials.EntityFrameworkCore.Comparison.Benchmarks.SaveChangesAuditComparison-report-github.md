```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                               | Count | State    | Mean         | Error      | StdDev     | Median       | Ratio | RatioSD | Gen0     | Gen1     | Allocated   | Alloc Ratio |
|------------------------------------- |------ |--------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|---------:|---------:|------------:|------------:|
| **Ef_SaveChanges**                       | **1**     | **Modified** |     **57.48 μs** |   **0.136 μs** |   **0.127 μs** |     **57.45 μs** |  **1.00** |    **0.00** |   **0.7324** |   **0.1831** |    **12.27 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Modified |     62.66 μs |   0.270 μs |   0.239 μs |     62.60 μs |  1.09 |    0.00 |   0.7324 |   0.1221 |    13.61 KB |        1.11 |
| CSharpEssentials_AuditInterceptor    | 1     | Modified |     63.74 μs |   0.142 μs |   0.126 μs |     63.72 μs |  1.11 |    0.00 |   0.8545 |   0.1221 |    14.08 KB |        1.15 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Modified |     64.33 μs |   0.244 μs |   0.228 μs |     64.32 μs |  1.12 |    0.00 |   0.8545 |   0.1221 |    14.02 KB |        1.14 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1**     | **Added**    |     **67.80 μs** |   **0.220 μs** |   **0.195 μs** |     **67.77 μs** |  **1.00** |    **0.00** |   **0.9766** |   **0.1221** |       **16 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Added    |     69.23 μs |   0.189 μs |   0.167 μs |     69.18 μs |  1.02 |    0.00 |   0.9766 |        - |    16.63 KB |        1.04 |
| CSharpEssentials_AuditInterceptor    | 1     | Added    |     70.90 μs |   0.272 μs |   0.227 μs |     70.86 μs |  1.05 |    0.00 |   0.9766 |        - |     17.1 KB |        1.07 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Added    |     71.32 μs |   0.143 μs |   0.120 μs |     71.33 μs |  1.05 |    0.00 |   0.9766 |        - |     17.1 KB |        1.07 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Modified** |  **1,883.62 μs** |   **8.125 μs** |   **6.784 μs** |  **1,884.66 μs** |  **1.00** |    **0.00** |  **31.2500** |        **-** |   **732.24 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Modified |  2,146.02 μs |  11.782 μs |   9.838 μs |  2,144.25 μs |  1.14 |    0.01 |  46.8750 |  15.6250 |   845.74 KB |        1.15 |
| CSharpEssentials_AuditInterceptor    | 100   | Modified |  2,126.53 μs |  17.590 μs |  14.688 μs |  2,122.34 μs |  1.13 |    0.01 |  46.8750 |  15.6250 |   846.92 KB |        1.16 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Modified |  2,173.91 μs |   6.254 μs |   5.544 μs |  2,174.34 μs |  1.15 |    0.00 |  46.8750 |  15.6250 |   847.08 KB |        1.16 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Added**    |  **2,525.16 μs** |   **8.518 μs** |   **7.113 μs** |  **2,524.15 μs** |  **1.00** |    **0.00** |  **62.5000** |  **15.6250** |  **1094.09 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Added    |  2,668.68 μs |  38.104 μs |  35.643 μs |  2,667.87 μs |  1.06 |    0.01 |  62.5000 |        - |  1146.55 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 100   | Added    |  2,707.52 μs |  19.014 μs |  16.855 μs |  2,699.86 μs |  1.07 |    0.01 |  62.5000 |        - |  1147.81 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Added    |  2,735.09 μs |  10.856 μs |   9.623 μs |  2,733.58 μs |  1.08 |    0.00 |  62.5000 |        - |  1147.87 KB |        1.05 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Modified** | **18,860.87 μs** |  **73.237 μs** |  **68.506 μs** | **18,854.31 μs** |  **1.00** |    **0.00** | **437.5000** | **218.7500** |  **7262.28 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Modified | 22,548.00 μs | 179.656 μs | 168.050 μs | 22,511.37 μs |  1.20 |    0.01 | 500.0000 | 281.2500 |  8395.23 KB |        1.16 |
| CSharpEssentials_AuditInterceptor    | 1000  | Modified | 21,914.24 μs |  68.665 μs |  64.229 μs | 21,923.03 μs |  1.16 |    0.01 | 500.0000 | 281.2500 |  8403.51 KB |        1.16 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Modified | 22,563.81 μs | 450.716 μs | 674.611 μs | 22,218.39 μs |  1.20 |    0.04 | 500.0000 | 250.0000 |  8403.57 KB |        1.16 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Added**    | **25,902.24 μs** |  **81.807 μs** |  **76.522 μs** | **25,895.47 μs** |  **1.00** |    **0.00** | **656.2500** | **312.5000** | **10879.47 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Added    | 27,625.17 μs | 542.841 μs | 705.846 μs | 27,303.75 μs |  1.07 |    0.03 | 666.6667 | 333.3333 | 11403.25 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 1000  | Added    | 28,275.79 μs | 553.776 μs | 720.066 μs | 28,121.37 μs |  1.09 |    0.03 | 666.6667 | 333.3333 | 11411.65 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Added    | 27,447.65 μs | 537.469 μs | 787.815 μs | 27,056.80 μs |  1.06 |    0.03 | 666.6667 | 333.3333 |  11411.7 KB |        1.05 |
