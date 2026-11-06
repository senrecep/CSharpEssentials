```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                               | Count | State    | Mean         | Error      | StdDev     | Median       | Ratio | RatioSD | Gen0     | Gen1     | Allocated   | Alloc Ratio |
|------------------------------------- |------ |--------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|---------:|---------:|------------:|------------:|
| **Ef_SaveChanges**                       | **1**     | **Modified** |     **68.93 μs** |   **0.251 μs** |   **0.210 μs** |     **68.90 μs** |  **1.00** |    **0.00** |   **0.7324** |   **0.1221** |    **12.91 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Modified |     75.87 μs |   0.195 μs |   0.183 μs |     75.85 μs |  1.10 |    0.00 |   0.8545 |   0.1221 |    14.22 KB |        1.10 |
| CSharpEssentials_AuditInterceptor    | 1     | Modified |     77.42 μs |   0.240 μs |   0.224 μs |     77.47 μs |  1.12 |    0.00 |   0.8545 |   0.1221 |    14.75 KB |        1.14 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Modified |     77.42 μs |   0.334 μs |   0.279 μs |     77.39 μs |  1.12 |    0.01 |   0.8545 |   0.1221 |    14.75 KB |        1.14 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1**     | **Added**    |     **77.41 μs** |   **0.343 μs** |   **0.304 μs** |     **77.30 μs** |  **1.00** |    **0.01** |   **0.9766** |        **-** |     **16.4 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1     | Added    |     81.03 μs |   0.273 μs |   0.213 μs |     81.01 μs |  1.05 |    0.00 |   0.9766 |        - |    17.11 KB |        1.04 |
| CSharpEssentials_AuditInterceptor    | 1     | Added    |     82.81 μs |   0.798 μs |   0.666 μs |     82.50 μs |  1.07 |    0.01 |   0.9766 |        - |    17.64 KB |        1.08 |
| CSharpEssentials_BaseDbContext_Audit | 1     | Added    |     84.01 μs |   0.996 μs |   0.931 μs |     84.05 μs |  1.09 |    0.01 |   0.9766 |        - |    17.64 KB |        1.08 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Modified** |  **2,118.65 μs** |   **6.363 μs** |   **5.314 μs** |  **2,117.71 μs** |  **1.00** |    **0.00** |  **31.2500** |        **-** |   **761.45 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Modified |  2,608.34 μs |   4.740 μs |   3.958 μs |  2,609.00 μs |  1.23 |    0.00 |  46.8750 |  15.6250 |   869.49 KB |        1.14 |
| CSharpEssentials_AuditInterceptor    | 100   | Modified |  2,584.58 μs |  12.109 μs |  10.734 μs |  2,580.95 μs |  1.22 |    0.01 |  46.8750 |  15.6250 |   872.08 KB |        1.15 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Modified |  2,587.64 μs |   8.397 μs |   7.443 μs |  2,586.19 μs |  1.22 |    0.00 |  46.8750 |  15.6250 |   872.15 KB |        1.15 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **100**   | **Added**    |  **3,020.00 μs** |  **12.907 μs** |  **11.442 μs** |  **3,020.70 μs** |  **1.00** |    **0.01** |  **62.5000** |  **15.6250** |  **1109.89 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 100   | Added    |  3,112.73 μs |  12.740 μs |  11.294 μs |  3,114.06 μs |  1.03 |    0.01 |  62.5000 |  15.6250 |  1158.55 KB |        1.04 |
| CSharpEssentials_AuditInterceptor    | 100   | Added    |  3,179.14 μs |  17.073 μs |  15.134 μs |  3,179.12 μs |  1.05 |    0.01 |  62.5000 |  31.2500 |  1161.14 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 100   | Added    |  3,196.00 μs |  12.259 μs |  10.867 μs |  3,199.80 μs |  1.06 |    0.01 |  62.5000 |  31.2500 |  1161.21 KB |        1.05 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Modified** | **20,431.85 μs** | **122.193 μs** | **108.321 μs** | **20,427.02 μs** |  **1.00** |    **0.01** | **437.5000** | **281.2500** |  **7551.55 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Modified | 25,690.91 μs |  88.040 μs |  78.046 μs | 25,684.32 μs |  1.26 |    0.01 | 500.0000 | 343.7500 |  8629.91 KB |        1.14 |
| CSharpEssentials_AuditInterceptor    | 1000  | Modified | 26,303.08 μs |  89.877 μs |  79.673 μs | 26,289.01 μs |  1.29 |    0.01 | 500.0000 | 343.7500 |  8646.56 KB |        1.15 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Modified | 26,573.26 μs | 518.455 μs | 597.054 μs | 26,185.28 μs |  1.30 |    0.03 | 500.0000 | 250.0000 |  8646.85 KB |        1.15 |
|                                      |       |          |              |            |            |              |       |         |          |          |             |             |
| **Ef_SaveChanges**                       | **1000**  | **Added**    | **30,334.55 μs** |  **92.125 μs** |  **86.174 μs** | **30,359.36 μs** |  **1.00** |    **0.00** | **656.2500** | **437.5000** | **11035.93 KB** |        **1.00** |
| HandWritten_SavingChangesOverride    | 1000  | Added    | 31,481.34 μs | 108.122 μs |  95.847 μs | 31,468.39 μs |  1.04 |    0.00 | 687.5000 | 500.0000 | 11520.56 KB |        1.04 |
| CSharpEssentials_AuditInterceptor    | 1000  | Added    | 30,864.49 μs |  73.320 μs |  68.583 μs | 30,878.10 μs |  1.02 |    0.00 | 687.5000 | 500.0000 | 11537.19 KB |        1.05 |
| CSharpEssentials_BaseDbContext_Audit | 1000  | Added    | 31,591.57 μs | 146.931 μs | 122.694 μs | 31,558.36 μs |  1.04 |    0.00 | 687.5000 | 500.0000 | 11537.29 KB |        1.05 |
