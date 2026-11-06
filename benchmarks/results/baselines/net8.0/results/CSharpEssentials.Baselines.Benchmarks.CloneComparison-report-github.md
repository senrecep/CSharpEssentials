```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                | Categories | Count | Mean         | Error      | StdDev     | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |------ |-------------:|-----------:|-----------:|------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_CloneToArray**         | **Array**      | **10**    |    **163.13 ns** |   **0.819 ns** |   **0.726 ns** |  **1.00** | **0.0281** |      **-** |     **472 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 10    |     91.89 ns |   0.829 ns |   0.775 ns |  0.56 | 0.0253 |      - |     424 B |        0.90 |
| Bcl_ArrayConvertAll                   | Array      | 10    |    105.26 ns |   0.671 ns |   0.628 ns |  0.65 | 0.0253 |      - |     424 B |        0.90 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_CloneToArray**         | **Array**      | **1000**  | **12,922.80 ns** |  **55.247 ns** |  **43.133 ns** |  **1.00** | **2.3804** | **0.2899** |   **40072 B** |        **1.00** |
| HandWritten_ArrayLoop                 | Array      | 1000  |  9,468.39 ns | 103.742 ns |  97.041 ns |  0.73 | 2.3804 | 0.3052 |   40024 B |        1.00 |
| Bcl_ArrayConvertAll                   | Array      | 1000  |  9,744.39 ns |  85.768 ns |  80.228 ns |  0.75 | 2.3804 | 0.3052 |   40024 B |        1.00 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **10**    |    **304.36 ns** |   **1.616 ns** |   **1.512 ns** |  **1.00** | **0.0420** |      **-** |     **704 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 10    |    201.75 ns |   0.696 ns |   0.617 ns |  0.66 | 0.0386 |      - |     648 B |        0.92 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_LazyCloneToList**      | **LazySource** | **1000**  | **19,657.20 ns** | **130.000 ns** | **115.242 ns** |  **1.00** | **2.8992** | **0.3662** |   **48656 B** |        **1.00** |
| HandWritten_LazyForeach               | LazySource | 1000  | 13,127.94 ns |  49.902 ns |  41.670 ns |  0.67 | 2.8992 | 0.4120 |   48600 B |        1.00 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **10**    |    **188.21 ns** |   **0.381 ns** |   **0.338 ns** |  **1.00** | **0.0315** |      **-** |     **528 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 10    |    120.34 ns |   1.062 ns |   0.887 ns |  0.64 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_ConvertAll                        | List       | 10    |    116.03 ns |   0.578 ns |   0.540 ns |  0.62 | 0.0272 |      - |     456 B |        0.86 |
| Bcl_LinqSelectToList                  | List       | 10    |    154.65 ns |   0.944 ns |   0.883 ns |  0.82 | 0.0315 |      - |     528 B |        1.00 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_CloneToList**          | **List**       | **1000**  | **12,052.88 ns** |  **89.421 ns** |  **83.645 ns** |  **1.00** | **2.3956** | **0.2899** |   **40128 B** |        **1.00** |
| HandWritten_ForLoop                   | List       | 1000  |  9,735.70 ns |  88.009 ns |  82.324 ns |  0.81 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_ConvertAll                        | List       | 1000  | 10,039.36 ns | 131.471 ns | 122.978 ns |  0.83 | 2.3804 | 0.3052 |   40056 B |        1.00 |
| Bcl_LinqSelectToList                  | List       | 1000  |  9,322.25 ns |  68.140 ns |  63.738 ns |  0.77 | 2.3956 | 0.2899 |   40128 B |        1.00 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **10**    |    **386.03 ns** |   **1.558 ns** |   **1.458 ns** |  **1.00** | **0.0477** |      **-** |     **800 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 10    |    131.95 ns |   0.810 ns |   0.757 ns |  0.34 | 0.0272 |      - |     456 B |        0.57 |
|                                       |            |       |              |            |            |       |        |        |           |             |
| **CSharpEssentials_QueryableCloneToList** | **Queryable**  | **1000**  | **23,821.94 ns** |  **55.270 ns** |  **48.995 ns** |  **1.00** | **2.8992** | **0.3967** |   **48752 B** |        **1.00** |
| HandWritten_ForLoopFromList           | Queryable  | 1000  |  9,694.35 ns |  86.511 ns |  80.923 ns |  0.41 | 2.3804 | 0.3052 |   40056 B |        0.82 |
