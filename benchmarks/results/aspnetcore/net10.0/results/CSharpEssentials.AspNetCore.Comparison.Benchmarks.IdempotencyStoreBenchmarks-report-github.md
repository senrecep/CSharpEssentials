```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                       | Categories  | PayloadSize | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|--------------------------------------------- |------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **0**           |  **2,684.14 ns** |  **17.247 ns** |  **15.289 ns** |  **1.00** |    **0.01** | **0.1488** | **0.1450** | **0.0763** |    **2528 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 0           |    280.68 ns |   1.803 ns |   1.686 ns |  0.10 |    0.00 | 0.0353 |      - |      - |     592 B |        0.23 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **1024**        |  **3,350.96 ns** |  **58.384 ns** |  **54.613 ns** |  **1.00** |    **0.02** | **0.3357** | **0.1984** | **0.0687** |    **4646 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 1024        |    342.49 ns |   2.219 ns |   2.076 ns |  0.10 |    0.00 | 0.0978 |      - |      - |    1640 B |        0.35 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_Distributed_ReserveComplete** | **Distributed** | **65536**       | **27,469.85 ns** | **462.734 ns** | **475.194 ns** |  **1.00** |    **0.02** | **8.9111** | **8.8501** | **0.9766** |  **133674 B** |        **1.00** |
| CSharpEssentials_Distributed_Replay          | Distributed | 65536       |  3,347.91 ns |  62.309 ns |  61.196 ns |  0.12 |    0.00 | 3.9368 | 0.0038 |      - |   66152 B |        0.49 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **0**           |  **1,238.02 ns** |   **5.915 ns** |   **5.533 ns** |  **1.00** |    **0.01** | **0.0248** | **0.0191** |      **-** |     **432 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 0           |     71.59 ns |   0.096 ns |   0.085 ns |  0.06 |    0.00 | 0.0033 |      - |      - |      56 B |        0.13 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 0           |  1,062.08 ns |   3.082 ns |   2.883 ns |  0.86 |    0.00 | 0.0210 |      - |      - |     376 B |        0.87 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **1024**        |  **1,428.43 ns** |   **9.325 ns** |   **8.723 ns** |  **1.00** |    **0.01** | **0.0877** | **0.0801** |      **-** |    **1480 B** |        **1.00** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 1024        |     73.01 ns |   0.225 ns |   0.200 ns |  0.05 |    0.00 | 0.0033 |      - |      - |      56 B |        0.04 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 1024        |  1,074.98 ns |   2.441 ns |   2.283 ns |  0.75 |    0.00 | 0.0210 |      - |      - |     376 B |        0.25 |
|                                              |             |             |              |            |            |       |         |        |        |        |           |             |
| **CSharpEssentials_InMemory_ReserveComplete**    | **InMemory**    | **65536**       | **14,640.26 ns** | **247.400 ns** | **231.418 ns** | **1.000** |    **0.02** | **4.5471** | **4.5166** | **0.6104** |   **65996 B** |       **1.000** |
| CSharpEssentials_InMemory_ReserveHit         | InMemory    | 65536       |     74.91 ns |   0.118 ns |   0.105 ns | 0.005 |    0.00 | 0.0033 |      - |      - |      56 B |       0.001 |
| CSharpEssentials_InMemory_ReserveRelease     | InMemory    | 65536       |  1,064.66 ns |   3.934 ns |   3.680 ns | 0.073 |    0.00 | 0.0210 |      - |      - |     376 B |       0.006 |
