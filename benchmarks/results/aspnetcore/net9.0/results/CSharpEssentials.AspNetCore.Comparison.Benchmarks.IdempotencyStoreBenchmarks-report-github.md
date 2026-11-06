```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **2,971.91 ns** |  **33.801 ns** |  **29.963 ns** |  **1.00** |    **0.01** | **0.1717** | **0.1526** | **0.0992** |    **2909 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    291.85 ns |   2.847 ns |   2.663 ns |  0.10 |    0.00 | 0.0353 |      - |      - |     592 B |        0.20 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **3,665.57 ns** |  **65.001 ns** |  **60.802 ns** |  **1.00** |    **0.02** | **0.3586** | **0.2060** | **0.0687** |    **5034 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    364.59 ns |   4.470 ns |   4.182 ns |  0.10 |    0.00 | 0.0978 |      - |      - |    1640 B |        0.33 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **28,134.35 ns** | **336.765 ns** | **298.534 ns** |  **1.00** |    **0.01** | **8.7585** | **8.7280** | **0.7935** |  **134057 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |  3,416.61 ns |  42.095 ns |  39.375 ns |  0.12 |    0.00 | 3.9368 | 0.0038 |      - |   66152 B |        0.49 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |  **1,361.78 ns** |  **15.454 ns** |  **14.456 ns** |  **1.00** |    **0.01** | **0.0248** | **0.0191** |      **-** |     **432 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     73.31 ns |   0.585 ns |   0.547 ns |  0.05 |    0.00 | 0.0033 |      - |      - |      56 B |        0.13 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |  1,125.09 ns |   2.387 ns |   1.863 ns |  0.83 |    0.01 | 0.0210 |      - |      - |     376 B |        0.87 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |  **1,521.67 ns** |  **12.580 ns** |  **11.152 ns** |  **1.00** |    **0.01** | **0.0877** | **0.0801** |      **-** |    **1480 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     67.43 ns |   0.350 ns |   0.328 ns |  0.04 |    0.00 | 0.0033 |      - |      - |      56 B |        0.04 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |  1,138.23 ns |   3.001 ns |   2.807 ns |  0.75 |    0.01 | 0.0210 |      - |      - |     376 B |        0.25 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **14,774.55 ns** | **214.186 ns** | **200.350 ns** | **1.000** |    **0.02** | **4.4556** | **4.4250** | **0.5188** |   **65995 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     66.71 ns |   0.235 ns |   0.220 ns | 0.005 |    0.00 | 0.0033 |      - |      - |      56 B |       0.001 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |  1,123.28 ns |   5.942 ns |   5.558 ns | 0.076 |    0.00 | 0.0210 |      - |      - |     376 B |       0.006 |
