```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **3,146.28 ns** |  **31.657 ns** |  **29.612 ns** |  **1.00** |    **0.01** | **0.2060** | **0.1755** | **0.1221** |    **3456 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    361.34 ns |   1.830 ns |   1.711 ns |  0.11 |    0.00 | 0.0677 |      - |      - |    1136 B |        0.33 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **3,995.13 ns** |  **53.008 ns** |  **49.583 ns** |  **1.00** |    **0.02** | **0.4044** | **0.2365** | **0.0839** |    **5576 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    425.28 ns |   2.993 ns |   2.800 ns |  0.11 |    0.00 | 0.1302 |      - |      - |    2184 B |        0.39 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **27,488.55 ns** | **544.356 ns** | **559.013 ns** |  **1.00** |    **0.03** | **8.8196** | **8.7891** | **0.8240** |  **134600 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |  3,355.09 ns |  55.326 ns |  51.752 ns |  0.12 |    0.00 | 3.9673 | 0.0114 |      - |   66696 B |        0.50 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |  **1,383.85 ns** |   **6.452 ns** |   **5.387 ns** |  **1.00** |    **0.01** | **0.0248** | **0.0191** |      **-** |     **432 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     67.18 ns |   0.466 ns |   0.436 ns |  0.05 |    0.00 | 0.0033 |      - |      - |      56 B |        0.13 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |  1,187.48 ns |   3.568 ns |   2.979 ns |  0.86 |    0.00 | 0.0210 |      - |      - |     376 B |        0.87 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |  **1,586.28 ns** |   **6.028 ns** |   **5.033 ns** |  **1.00** |    **0.00** | **0.0877** | **0.0801** |      **-** |    **1480 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     67.84 ns |   0.134 ns |   0.126 ns |  0.04 |    0.00 | 0.0033 |      - |      - |      56 B |        0.04 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |  1,187.33 ns |   1.852 ns |   1.642 ns |  0.75 |    0.00 | 0.0210 |      - |      - |     376 B |        0.25 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **14,835.13 ns** | **290.842 ns** | **311.198 ns** | **1.000** |    **0.03** | **4.4250** | **4.3945** | **0.4883** |   **65995 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     67.42 ns |   0.165 ns |   0.147 ns | 0.005 |    0.00 | 0.0033 |      - |      - |      56 B |       0.001 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |  1,173.37 ns |   3.754 ns |   3.512 ns | 0.079 |    0.00 | 0.0210 |      - |      - |     376 B |       0.006 |
