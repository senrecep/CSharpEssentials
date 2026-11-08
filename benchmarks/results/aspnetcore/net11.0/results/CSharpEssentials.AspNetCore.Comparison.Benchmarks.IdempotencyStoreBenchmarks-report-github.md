```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **1,664.53 ns** |  **21.544 ns** |  **20.152 ns** |  **1.00** |    **0.02** | **0.1450** | **0.1373** | **0.0687** |    **2525 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    225.37 ns |   1.271 ns |   1.127 ns |  0.14 |    0.00 | 0.0353 |      - |      - |     592 B |        0.23 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **1,967.30 ns** |  **29.177 ns** |  **25.864 ns** |  **1.00** |    **0.02** | **0.2136** | **0.2060** | **0.1450** |    **3549 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    225.07 ns |   0.996 ns |   0.883 ns |  0.11 |    0.00 | 0.0353 |      - |      - |     592 B |        0.17 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **18,621.39 ns** | **367.876 ns** | **594.051 ns** |  **1.00** |    **0.04** | **4.7302** | **4.6997** | **0.7019** |   **68070 B** |       **1.000** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |    234.67 ns |   1.073 ns |   1.004 ns |  0.01 |    0.00 | 0.0353 |      - |      - |     592 B |       0.009 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |    **390.45 ns** |   **1.517 ns** |   **1.419 ns** |  **1.00** |    **0.00** | **0.0262** | **0.0205** |      **-** |     **440 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     52.25 ns |   0.059 ns |   0.056 ns |  0.13 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |    228.14 ns |   1.064 ns |   0.995 ns |  0.58 |    0.00 | 0.0196 |      - |      - |     328 B |        0.75 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |    **531.25 ns** |   **3.636 ns** |   **3.401 ns** |  **1.00** |    **0.01** | **0.0887** | **0.0839** |      **-** |    **1488 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     52.27 ns |   0.094 ns |   0.088 ns |  0.10 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |    284.32 ns |   1.818 ns |   1.701 ns |  0.54 |    0.00 | 0.0196 |      - |      - |     328 B |        0.22 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **13,651.82 ns** | **271.742 ns** | **240.892 ns** | **1.000** |    **0.02** | **4.4250** | **4.3945** | **0.4883** |   **66003 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     53.12 ns |   0.076 ns |   0.067 ns | 0.004 |    0.00 |      - |      - |      - |         - |       0.000 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |    272.31 ns |   2.134 ns |   1.996 ns | 0.020 |    0.00 | 0.0196 |      - |      - |     328 B |       0.005 |
