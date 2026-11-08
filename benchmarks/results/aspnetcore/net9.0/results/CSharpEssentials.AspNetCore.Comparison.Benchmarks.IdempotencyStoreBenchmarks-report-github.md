```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **2,797.23 ns** |  **14.085 ns** |  **12.486 ns** |  **1.00** |    **0.01** | **0.1717** | **0.1526** | **0.0992** |    **2909 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    298.79 ns |   2.410 ns |   2.254 ns |  0.11 |    0.00 | 0.0353 |      - |      - |     592 B |        0.20 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **3,115.52 ns** |  **28.656 ns** |  **26.805 ns** |  **1.00** |    **0.01** | **0.2365** | **0.2327** | **0.1869** |    **3939 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    272.59 ns |   2.432 ns |   2.275 ns |  0.09 |    0.00 | 0.0353 |      - |      - |     592 B |        0.15 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **20,888.21 ns** | **411.248 ns** | **709.383 ns** |  **1.00** |    **0.05** | **4.9133** | **4.8828** | **0.8545** |   **68444 B** |       **1.000** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |    274.99 ns |   2.513 ns |   2.351 ns |  0.01 |    0.00 | 0.0353 |      - |      - |     592 B |       0.009 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |    **473.88 ns** |   **3.941 ns** |   **3.687 ns** |  **1.00** |    **0.01** | **0.0262** | **0.0200** |      **-** |     **440 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     60.56 ns |   0.089 ns |   0.084 ns |  0.13 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |    289.54 ns |   2.195 ns |   2.053 ns |  0.61 |    0.01 | 0.0196 |      - |      - |     328 B |        0.75 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |    **617.71 ns** |   **7.414 ns** |   **6.935 ns** |  **1.00** |    **0.02** | **0.0887** | **0.0839** |      **-** |    **1488 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     59.61 ns |   0.077 ns |   0.072 ns |  0.10 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |    287.33 ns |   1.691 ns |   1.499 ns |  0.47 |    0.01 | 0.0196 |      - |      - |     328 B |        0.22 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **13,837.52 ns** | **262.683 ns** | **232.862 ns** | **1.000** |    **0.02** | **4.4556** | **4.4250** | **0.5188** |   **66003 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     60.07 ns |   0.100 ns |   0.088 ns | 0.004 |    0.00 |      - |      - |      - |         - |       0.000 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |    282.42 ns |   1.863 ns |   1.743 ns | 0.020 |    0.00 | 0.0196 |      - |      - |     328 B |       0.005 |
