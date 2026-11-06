```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                | Categories | Count | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_CloneToArray**         | **Array**      | **10**    |    **162.07 ns** |   **0.758 ns** |   **0.709 ns** |  **1.00** |    **0.01** | **0.0281** |      **-** |     **472 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 10    |     98.47 ns |   0.391 ns |   0.347 ns |  0.61 |    0.00 | 0.0253 |      - |     424 B |        0.90 |
| Bcl_ArrayConvertAll                   | Array      | 10    |    110.31 ns |   0.707 ns |   0.590 ns |  0.68 |    0.00 | 0.0253 |      - |     424 B |        0.90 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToArray**         | **Array**      | **1000**  | **13,597.92 ns** |  **59.157 ns** |  **55.335 ns** |  **1.00** |    **0.01** | **2.3804** | **0.2899** |   **40072 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 1000  |  9,389.75 ns |  62.763 ns |  58.709 ns |  0.69 |    0.00 | 2.3804 | 0.3052 |   40024 B |        1.00 |
| Bcl_ArrayConvertAll                   | Array      | 1000  | 10,436.58 ns |  54.324 ns |  50.815 ns |  0.77 |    0.00 | 2.3804 | 0.3052 |   40024 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **10**    |    **273.85 ns** |   **1.206 ns** |   **1.007 ns** |  **1.00** |    **0.01** | **0.0305** |      **-** |     **512 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 10    |    215.38 ns |   1.273 ns |   1.191 ns |  0.79 |    0.01 | 0.0386 |      - |     648 B |        1.27 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **1000**  | **19,229.59 ns** | **111.782 ns** | **104.561 ns** |  **1.00** |    **0.01** | **2.3804** |      **-** |   **40112 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 1000  | 13,705.07 ns |  97.122 ns |  86.096 ns |  0.71 |    0.01 | 2.8992 | 0.4120 |   48600 B |        1.21 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **10**    |    **180.34 ns** |   **1.194 ns** |   **1.117 ns** |  **1.00** |    **0.01** | **0.0315** |      **-** |     **528 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 10    |    107.85 ns |   0.313 ns |   0.293 ns |  0.60 |    0.00 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_ConvertAll                        | List       | 10    |    118.16 ns |   1.206 ns |   1.069 ns |  0.66 |    0.01 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_LinqSelectToList                  | List       | 10    |    155.58 ns |   1.127 ns |   1.054 ns |  0.86 |    0.01 | 0.0315 |      - |     528 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **1000**  | **13,696.53 ns** | **114.645 ns** | **107.239 ns** |  **1.00** |    **0.01** | **2.3956** | **0.2899** |   **40128 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 1000  |  9,732.65 ns |  78.265 ns |  69.380 ns |  0.71 |    0.01 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_ConvertAll                        | List       | 1000  | 10,739.28 ns | 105.679 ns |  98.853 ns |  0.78 |    0.01 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_LinqSelectToList                  | List       | 1000  | 11,865.62 ns |  78.819 ns |  73.727 ns |  0.87 |    0.01 | 2.3956 | 0.2899 |   40128 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **10**    |    **352.68 ns** |   **5.120 ns** |   **4.789 ns** |  **1.00** |    **0.02** | **0.0477** |      **-** |     **800 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 10    |    108.68 ns |   0.577 ns |   0.540 ns |  0.31 |    0.00 | 0.0272 |      - |     456 B |        0.57 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **1000**  | **20,970.64 ns** | **142.506 ns** | **133.300 ns** |  **1.00** |    **0.01** | **2.8992** | **0.3967** |   **48752 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 1000  |  9,716.74 ns | 128.924 ns | 120.596 ns |  0.46 |    0.01 | 2.3804 | 0.3052 |   40056 B |        0.82 |
