```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                               | Count | State    | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0     | Gen1     | Allocated   | Alloc Ratio |
|------------------------------------- |------ |--------- |-------------:|-----------:|-----------:|------:|--------:|---------:|---------:|------------:|------------:|
| **Ef_SaveChanges**                       | **1**     | **Modified** |     **65.09 μs** |   **0.118 μs** |   **0.105 μs** |  **1.00** |    **0.00** |   **0.7324** |   **0.1221** |    **12.96 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Modified |     71.44 μs |   0.203 μs |   0.170 μs |  1.10 |    0.00 |   0.8545 |   0.1221 |    14.32 KB |        1.10 |
| CSharpEssentials_AuditInterceptor    | 1     | Modified |     71.12 μs |   0.228 μs |   0.202 μs |  1.09 |    0.00 |   0.8545 |   0.1221 |    14.83 KB |        1.14 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Modified |     72.54 μs |   0.230 μs |   0.215 μs |  1.11 |    0.00 |   0.8545 |   0.1221 |    14.83 KB |        1.14 |
|                                      |       |          |              |            |            |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1**     | **Added**    |     **72.81 μs** |   **0.329 μs** |   **0.292 μs** |  **1.00** |    **0.01** |   **0.9766** |   **0.1221** |    **16.38 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Added    |     76.53 μs |   0.107 μs |   0.095 μs |  1.05 |    0.00 |   0.9766 |   0.1221 |    17.13 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 1     | Added    |     78.32 μs |   0.233 μs |   0.218 μs |  1.08 |    0.01 |   0.9766 |   0.1221 |    17.63 KB |        1.08 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Added    |     78.89 μs |   0.443 μs |   0.393 μs |  1.08 |    0.01 |   0.9766 |        - |    17.63 KB |        1.08 |
|                                      |       |          |              |            |            |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Modified** |  **2,093.95 μs** |   **6.884 μs** |   **6.102 μs** |  **1.00** |    **0.00** |  **46.8750** |  **15.6250** |    **766.2 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Modified |  2,483.78 μs |   5.928 μs |   4.950 μs |  1.19 |    0.00 |  46.8750 |  15.6250 |   879.72 KB |        1.15 |
| CSharpEssentials_AuditInterceptor    | 100   | Modified |  2,422.40 μs |   7.012 μs |   5.855 μs |  1.16 |    0.00 |  46.8750 |  15.6250 |      881 KB |        1.15 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Modified |  2,435.90 μs |   9.430 μs |   8.359 μs |  1.16 |    0.01 |  46.8750 |  15.6250 |   881.07 KB |        1.15 |
|                                      |       |          |              |            |            |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Added**    |  **2,778.61 μs** |   **9.300 μs** |   **8.245 μs** |  **1.00** |    **0.00** |  **62.5000** |  **15.6250** |  **1107.62 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Added    |  2,987.42 μs |   8.580 μs |   7.165 μs |  1.08 |    0.00 |  62.5000 |  15.6250 |  1160.17 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 100   | Added    |  3,016.03 μs |  13.847 μs |  12.275 μs |  1.09 |    0.01 |  62.5000 |  15.6250 |  1161.45 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Added    |  2,998.46 μs |   8.549 μs |   7.138 μs |  1.08 |    0.00 |  62.5000 |  15.6250 |  1161.54 KB |        1.05 |
|                                      |       |          |              |            |            |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Modified** | **21,835.88 μs** |  **83.098 μs** |  **73.664 μs** |  **1.00** |    **0.00** | **437.5000** | **187.5000** |  **7598.48 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Modified | 25,424.01 μs | 109.498 μs |  91.436 μs |  1.16 |    0.01 | 531.2500 | 281.2500 |  8731.54 KB |        1.15 |
| CSharpEssentials_AuditInterceptor    | 1000  | Modified | 25,145.12 μs | 111.347 μs | 104.154 μs |  1.15 |    0.01 | 531.2500 | 281.2500 |  8739.85 KB |        1.15 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Modified | 24,599.40 μs |  82.648 μs |  73.266 μs |  1.13 |    0.00 | 531.2500 | 281.2500 |  8739.92 KB |        1.15 |
|                                      |       |          |              |            |            |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Added**    | **28,213.75 μs** | **111.220 μs** |  **92.874 μs** |  **1.00** |    **0.00** | **656.2500** | **375.0000** | **11012.56 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Added    | 30,462.29 μs | 592.982 μs | 659.098 μs |  1.08 |    0.02 | 666.6667 | 333.3333 | 11536.54 KB |        1.05 |
| CSharpEssentials_AuditInterceptor    | 1000  | Added    | 30,630.63 μs |  78.915 μs |  65.898 μs |  1.09 |    0.00 | 687.5000 | 375.0000 | 11544.57 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Added    | 30,138.90 μs | 171.952 μs | 134.249 μs |  1.07 |    0.01 | 687.5000 | 406.2500 |  11544.6 KB |        1.05 |
