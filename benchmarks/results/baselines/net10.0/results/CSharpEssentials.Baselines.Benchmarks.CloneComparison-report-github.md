```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                | Categories | Count | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_CloneToArray**         | **Array**      | **10**    |    **138.07 ns** |   **2.105 ns** |   **1.969 ns** |  **1.00** |    **0.02** | **0.0281** |      **-** |     **472 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 10    |     86.80 ns |   0.705 ns |   0.659 ns |  0.63 |    0.01 | 0.0253 |      - |     424 B |        0.90 |
| Bcl_ArrayConvertAll                   | Array      | 10    |     97.11 ns |   1.921 ns |   1.886 ns |  0.70 |    0.02 | 0.0253 |      - |     424 B |        0.90 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToArray**         | **Array**      | **1000**  | **10,987.40 ns** |  **67.912 ns** |  **60.202 ns** |  **1.00** |    **0.01** | **2.3804** | **0.2899** |   **40072 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 1000  |  7,961.21 ns |  98.880 ns |  92.492 ns |  0.72 |    0.01 | 2.3804 | 0.3052 |   40024 B |        1.00 |
| Bcl_ArrayConvertAll                   | Array      | 1000  |  9,522.98 ns | 152.202 ns | 142.370 ns |  0.87 |    0.01 | 2.3804 | 0.3052 |   40024 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **10**    |    **225.66 ns** |   **0.929 ns** |   **0.823 ns** |  **1.00** |    **0.00** | **0.0305** |      **-** |     **512 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 10    |    195.14 ns |   1.870 ns |   1.749 ns |  0.86 |    0.01 | 0.0386 |      - |     648 B |        1.27 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **1000**  | **15,185.36 ns** |  **86.798 ns** |  **76.944 ns** |  **1.00** |    **0.01** | **2.3956** | **0.0153** |   **40112 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 1000  | 12,767.87 ns |  99.914 ns |  93.460 ns |  0.84 |    0.01 | 2.8992 | 0.4120 |   48600 B |        1.21 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **10**    |    **135.04 ns** |   **1.216 ns** |   **1.078 ns** |  **1.00** |    **0.01** | **0.0315** |      **-** |     **528 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 10    |    104.65 ns |   0.614 ns |   0.512 ns |  0.78 |    0.01 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_ConvertAll                        | List       | 10    |    105.10 ns |   1.133 ns |   1.059 ns |  0.78 |    0.01 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_LinqSelectToList                  | List       | 10    |    144.85 ns |   2.867 ns |   2.394 ns |  1.07 |    0.02 | 0.0315 |      - |     528 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **1000**  | **10,736.55 ns** |  **86.979 ns** |  **77.104 ns** |  **1.00** |    **0.01** | **2.3956** | **0.2899** |   **40128 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 1000  |  8,815.49 ns |  75.603 ns |  70.719 ns |  0.82 |    0.01 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_ConvertAll                        | List       | 1000  |  9,138.58 ns |  78.057 ns |  73.015 ns |  0.85 |    0.01 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_LinqSelectToList                  | List       | 1000  | 11,893.25 ns | 224.105 ns | 209.628 ns |  1.11 |    0.02 | 2.3956 | 0.2899 |   40128 B |        1.00 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **10**    |    **244.74 ns** |   **2.612 ns** |   **2.444 ns** |  **1.00** |    **0.01** | **0.0477** |      **-** |     **800 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 10    |    157.37 ns |   0.945 ns |   0.884 ns |  0.64 |    0.01 | 0.0272 |      - |     456 B |        0.57 |
|                                       |            |       |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **1000**  | **13,267.85 ns** |  **67.121 ns** |  **62.785 ns** |  **1.00** |    **0.01** | **2.9144** | **0.4120** |   **48752 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 1000  |  9,033.44 ns | 136.996 ns | 128.146 ns |  0.68 |    0.01 | 2.3804 | 0.3052 |   40056 B |        0.82 |
