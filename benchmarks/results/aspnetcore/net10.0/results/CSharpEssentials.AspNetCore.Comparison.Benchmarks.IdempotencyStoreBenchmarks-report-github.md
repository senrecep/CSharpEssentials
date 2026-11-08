```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **2,604.87 ns** |  **23.311 ns** |  **21.805 ns** |  **1.00** |    **0.01** | **0.1488** | **0.1450** | **0.0801** |    **2528 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    258.46 ns |   2.869 ns |   2.684 ns |  0.10 |    0.00 | 0.0353 |      - |      - |     592 B |        0.23 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **2,948.76 ns** |  **44.842 ns** |  **41.945 ns** |  **1.00** |    **0.02** | **0.2136** | **0.2098** | **0.1678** |    **3555 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    257.98 ns |   2.115 ns |   1.875 ns |  0.09 |    0.00 | 0.0353 |      - |      - |     592 B |        0.17 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **19,415.83 ns** | **386.794 ns** | **635.514 ns** |  **1.00** |    **0.05** | **4.8218** | **4.7913** | **0.7935** |   **68069 B** |       **1.000** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |    273.20 ns |   1.398 ns |   1.307 ns |  0.01 |    0.00 | 0.0353 |      - |      - |     592 B |       0.009 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |    **388.51 ns** |   **3.513 ns** |   **2.742 ns** |  **1.00** |    **0.01** | **0.0262** | **0.0200** |      **-** |     **440 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     53.00 ns |   0.082 ns |   0.073 ns |  0.14 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |    258.66 ns |   1.636 ns |   1.530 ns |  0.67 |    0.01 | 0.0196 |      - |      - |     328 B |        0.75 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |    **597.33 ns** |   **4.549 ns** |   **4.255 ns** |  **1.00** |    **0.01** | **0.0887** | **0.0839** |      **-** |    **1488 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     50.97 ns |   0.065 ns |   0.057 ns |  0.09 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |    234.53 ns |   1.104 ns |   1.032 ns |  0.39 |    0.00 | 0.0196 |      - |      - |     328 B |        0.22 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **13,600.17 ns** | **107.945 ns** | **100.972 ns** | **1.000** |    **0.01** | **4.4708** | **4.4403** | **0.5341** |   **66004 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     50.67 ns |   0.071 ns |   0.059 ns | 0.004 |    0.00 |      - |      - |      - |         - |       0.000 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |    233.87 ns |   1.001 ns |   0.836 ns | 0.017 |    0.00 | 0.0196 |      - |      - |     328 B |       0.005 |
