```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|----------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **20.444 ns** |  **0.0971 ns** | **0.0908 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    19.054 ns |  0.1108 ns | 0.1037 ns |  0.93 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    20.251 ns |  0.2768 ns | 0.2589 ns |  0.99 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |    40.240 ns |  0.0146 ns | 0.0137 ns |  1.97 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    40.313 ns |  0.0422 ns | 0.0394 ns |  1.97 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    13.204 ns |  0.1052 ns | 0.0984 ns |  0.65 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |     8.974 ns |  0.0016 ns | 0.0014 ns |  0.44 |      - |         - |        0.00 |
|                                    |         |       |              |            |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **850.073 ns** |  **4.4953 ns** | **4.2049 ns** |  **1.00** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   830.676 ns |  4.1357 ns | 3.8685 ns |  0.98 | 0.0134 |     231 B |        0.97 |
| TaskSource_TaskHandler             | False   | True  |   848.767 ns |  3.3569 ns | 3.1400 ns |  1.00 | 0.0143 |     255 B |        1.07 |
| ValueTaskSource_SyncHandler        | False   | True  |   845.377 ns |  4.5406 ns | 4.2473 ns |  0.99 | 0.0181 |     310 B |        1.30 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   858.738 ns |  4.7769 ns | 4.4683 ns |  1.01 | 0.0191 |     325 B |        1.36 |
| Instance_TaskHandler               | False   | True  |    13.229 ns |  0.1131 ns | 0.1058 ns |  0.02 | 0.0048 |      80 B |        0.33 |
| Instance_ValueTaskHandler          | False   | True  |     8.968 ns |  0.0020 ns | 0.0016 ns |  0.01 |      - |         - |        0.00 |
|                                    |         |       |              |            |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **24.812 ns** |  **0.1623 ns** | **0.1518 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    17.999 ns |  0.2748 ns | 0.2570 ns |  0.73 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |    27.272 ns |  0.2595 ns | 0.2428 ns |  1.10 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |    35.001 ns |  0.1051 ns | 0.0983 ns |  1.41 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    39.671 ns |  0.0293 ns | 0.0274 ns |  1.60 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    24.581 ns |  0.2230 ns | 0.2086 ns |  0.99 | 0.0091 |     152 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    32.586 ns |  0.0488 ns | 0.0456 ns |  1.31 |      - |         - |        0.00 |
|                                    |         |       |              |            |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,355.686 ns** |  **5.0415 ns** | **4.7158 ns** |  **1.00** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   844.765 ns |  5.7305 ns | 5.3603 ns |  0.62 | 0.0134 |     231 B |        0.69 |
| TaskSource_TaskHandler             | True    | True  | 1,376.758 ns |  4.4941 ns | 4.2038 ns |  1.02 | 0.0210 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  |   842.380 ns |  4.9020 ns | 4.3455 ns |  0.62 | 0.0181 |     311 B |        0.93 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,452.138 ns | 10.5351 ns | 9.8545 ns |  1.07 | 0.0248 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  |   834.583 ns |  4.1954 ns | 3.9244 ns |  0.62 | 0.0124 |     223 B |        0.66 |
| Instance_ValueTaskHandler          | True    | True  |   841.881 ns |  4.7467 ns | 4.4401 ns |  0.62 | 0.0143 |     247 B |        0.74 |
