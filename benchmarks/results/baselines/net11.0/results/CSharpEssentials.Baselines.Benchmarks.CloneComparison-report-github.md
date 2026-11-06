```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                | Categories | Count | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_CloneToArray**         | **Array**      | **10**    |    **113.75 ns** |   **1.391 ns** |   **1.301 ns** |  **1.00** |    **0.02** | **0.0281** |      **-** |     **472 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 10    |     82.63 ns |   0.914 ns |   0.855 ns |  0.73 |    0.01 | 0.0253 |      - |     424 B |        0.90 |
| Bcl_ArrayConvertAll                   | Array      | 10    |     87.76 ns |   0.676 ns |   0.600 ns |  0.77 |    0.01 | 0.0253 |      - |     424 B |        0.90 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToArray**         | **Array**      | **1000**  |  **9,285.47 ns** | **141.648 ns** | **132.497 ns** |  **1.00** |    **0.02** | **2.3804** | **0.2899** |   **40072 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 1000  |  7,698.00 ns |  70.495 ns |  65.941 ns |  0.83 |    0.01 | 2.3880 | 0.3128 |   40024 B |        1.00 |
| Bcl_ArrayConvertAll                   | Array      | 1000  |  7,971.84 ns |  62.682 ns |  58.632 ns |  0.86 |    0.01 | 2.3804 | 0.3052 |   40024 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **10**    |    **205.41 ns** |   **1.533 ns** |   **1.359 ns** |  **1.00** |    **0.01** | **0.0305** |      **-** |     **512 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 10    |    180.11 ns |   1.974 ns |   1.847 ns |  0.88 |    0.01 | 0.0386 |      - |     648 B |        1.27 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **1000**  | **14,440.84 ns** | **160.831 ns** | **150.441 ns** |  **1.00** |    **0.01** | **2.3956** | **0.0153** |   **40112 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 1000  | 12,428.57 ns |  87.644 ns |  81.982 ns |  0.86 |    0.01 | 2.8992 | 0.4120 |   48600 B |        1.21 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **10**    |    **118.82 ns** |   **1.525 ns** |   **1.426 ns** |  **1.00** |    **0.02** | **0.0315** |      **-** |     **528 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 10    |    101.37 ns |   0.928 ns |   0.868 ns |  0.85 |    0.01 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_ConvertAll                        | List       | 10    |     95.33 ns |   0.422 ns |   0.394 ns |  0.80 |    0.01 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_LinqSelectToList                  | List       | 10    |    109.39 ns |   0.752 ns |   0.666 ns |  0.92 |    0.01 | 0.0315 |      - |     528 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **1000**  |  **8,737.23 ns** |  **41.473 ns** |  **32.380 ns** |  **1.00** |    **0.01** | **2.3956** | **0.2899** |   **40128 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 1000  |  8,170.05 ns |  63.928 ns |  59.798 ns |  0.94 |    0.01 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_ConvertAll                        | List       | 1000  |  8,424.16 ns |  83.615 ns |  78.213 ns |  0.96 |    0.01 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_LinqSelectToList                  | List       | 1000  |  8,436.10 ns |  87.518 ns |  77.583 ns |  0.97 |    0.01 | 2.3956 | 0.2899 |   40128 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **10**    |    **231.78 ns** |   **2.726 ns** |   **2.550 ns** |  **1.00** |    **0.02** | **0.0477** |      **-** |     **800 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 10    |    100.55 ns |   0.864 ns |   0.808 ns |  0.43 |    0.01 | 0.0272 |      - |     456 B |        0.57 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **1000**  | **13,371.17 ns** |  **77.466 ns** |  **72.462 ns** |  **1.00** |    **0.01** | **2.9144** | **0.4120** |   **48752 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 1000  |  8,058.16 ns |  57.244 ns |  53.546 ns |  0.60 |    0.01 | 2.3804 | 0.3052 |   40056 B |        0.82 |
