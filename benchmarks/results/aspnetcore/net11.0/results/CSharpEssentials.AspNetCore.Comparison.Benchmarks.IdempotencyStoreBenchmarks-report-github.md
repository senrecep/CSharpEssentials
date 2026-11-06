```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **1,723.39 ns** |  **16.448 ns** |  **14.580 ns** |  **1.00** |    **0.01** | **0.1450** | **0.1373** | **0.0687** |    **2525 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    233.12 ns |   2.281 ns |   2.134 ns |  0.14 |    0.00 | 0.0353 |      - |      - |     592 B |        0.23 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **2,407.61 ns** |  **47.720 ns** |  **53.041 ns** |  **1.00** |    **0.03** | **0.3204** | **0.1831** | **0.0610** |    **4629 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    295.20 ns |   1.978 ns |   1.851 ns |  0.12 |    0.00 | 0.0978 |      - |      - |    1640 B |        0.35 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **26,279.41 ns** | **519.080 ns** | **674.951 ns** |  **1.00** |    **0.04** | **8.8501** | **8.7891** | **0.9155** |  **133676 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |  3,039.07 ns |  30.282 ns |  25.286 ns |  0.12 |    0.00 | 3.9368 |      - |      - |   66152 B |        0.49 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |    **443.29 ns** |   **2.111 ns** |   **1.871 ns** |  **1.00** |    **0.01** | **0.0257** | **0.0205** |      **-** |     **432 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     58.04 ns |   0.120 ns |   0.094 ns |  0.13 |    0.00 | 0.0033 |      - |      - |      56 B |        0.13 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |    295.62 ns |   1.016 ns |   0.901 ns |  0.67 |    0.00 | 0.0224 |      - |      - |     376 B |        0.87 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |    **601.67 ns** |  **10.672 ns** |   **9.982 ns** |  **1.00** |    **0.02** | **0.0877** | **0.0820** |      **-** |    **1480 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     57.54 ns |   0.118 ns |   0.099 ns |  0.10 |    0.00 | 0.0033 |      - |      - |      56 B |        0.04 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |    288.99 ns |   0.682 ns |   0.569 ns |  0.48 |    0.01 | 0.0224 |      - |      - |     376 B |        0.25 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **14,033.57 ns** | **264.145 ns** | **324.394 ns** | **1.001** |    **0.03** | **4.4708** | **4.4403** | **0.5341** |   **65995 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     58.14 ns |   0.148 ns |   0.131 ns | 0.004 |    0.00 | 0.0033 |      - |      - |      56 B |       0.001 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |    292.36 ns |   2.811 ns |   2.492 ns | 0.021 |    0.00 | 0.0224 |      - |      - |     376 B |       0.006 |
