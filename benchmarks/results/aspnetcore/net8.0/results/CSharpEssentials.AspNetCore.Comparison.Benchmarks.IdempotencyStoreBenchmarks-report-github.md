```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **2,995.63 ns** |  **17.770 ns** |  **16.622 ns** |  **1.00** |    **0.01** | **0.2060** | **0.1831** | **0.1259** |    **3459 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    373.59 ns |   2.868 ns |   2.395 ns |  0.12 |    0.00 | 0.0677 |      - |      - |    1136 B |        0.33 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **3,425.28 ns** |  **47.057 ns** |  **44.017 ns** |  **1.00** |    **0.02** | **0.2975** | **0.2289** | **0.1411** |    **4478 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    363.83 ns |   2.896 ns |   2.709 ns |  0.11 |    0.00 | 0.0677 |      - |      - |    1136 B |        0.25 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **20,756.53 ns** | **399.439 ns** | **546.757 ns** |  **1.00** |    **0.04** | **4.8828** | **4.8523** | **0.7935** |   **69000 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |    341.48 ns |   2.205 ns |   2.063 ns |  0.02 |    0.00 | 0.0677 |      - |      - |    1136 B |        0.02 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |    **478.64 ns** |   **3.094 ns** |   **2.894 ns** |  **1.00** |    **0.01** | **0.0262** | **0.0200** |      **-** |     **440 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     59.98 ns |   0.069 ns |   0.061 ns |  0.13 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |    315.85 ns |   1.003 ns |   0.889 ns |  0.66 |    0.00 | 0.0196 |      - |      - |     328 B |        0.75 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |    **766.75 ns** |   **7.362 ns** |   **6.886 ns** |  **1.00** |    **0.01** | **0.0887** | **0.0839** |      **-** |    **1488 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     59.22 ns |   0.132 ns |   0.123 ns |  0.08 |    0.00 |      - |      - |      - |         - |        0.00 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |    299.14 ns |   2.403 ns |   2.248 ns |  0.39 |    0.00 | 0.0196 |      - |      - |     328 B |        0.22 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **14,055.95 ns** | **144.505 ns** | **135.170 ns** | **1.000** |    **0.01** | **4.4708** | **4.4403** | **0.5341** |   **66003 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     58.98 ns |   0.081 ns |   0.068 ns | 0.004 |    0.00 |      - |      - |      - |         - |       0.000 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |    288.09 ns |   1.304 ns |   1.156 ns | 0.020 |    0.00 | 0.0196 |      - |      - |     328 B |       0.005 |
