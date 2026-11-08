```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                    | Succeed | Yield | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-------- |------ |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **TaskSource_MatchAsync**     | **False**   | **False** |  **11.212 ns** | **0.0072 ns** | **0.0064 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| TaskSource_Match_Obsolete | False   | False |  11.466 ns | 0.0087 ns | 0.0081 ns |  1.02 |      - |         - |          NA |
| Instance_BindAsync        | False   | False |   9.327 ns | 0.0641 ns | 0.0600 ns |  0.83 | 0.0048 |      80 B |          NA |
| Instance_Bind_Obsolete    | False   | False |   9.469 ns | 0.1062 ns | 0.0994 ns |  0.84 | 0.0048 |      80 B |          NA |
|                           |         |       |            |           |           |       |        |           |             |
| **TaskSource_MatchAsync**     | **False**   | **True**  | **835.144 ns** | **5.2461 ns** | **4.9072 ns** |  **1.00** | **0.0134** |     **238 B** |        **1.00** |
| TaskSource_Match_Obsolete | False   | True  | 839.352 ns | 4.8627 ns | 4.5485 ns |  1.01 | 0.0134 |     238 B |        1.00 |
| Instance_BindAsync        | False   | True  |   9.042 ns | 0.0880 ns | 0.0780 ns |  0.01 | 0.0048 |      80 B |        0.34 |
| Instance_Bind_Obsolete    | False   | True  |   9.543 ns | 0.1264 ns | 0.1055 ns |  0.01 | 0.0048 |      80 B |        0.34 |
|                           |         |       |            |           |           |       |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **False** |  **16.243 ns** | **0.0896 ns** | **0.0749 ns** |  **1.00** | **0.0043** |      **72 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | False |  16.321 ns | 0.1029 ns | 0.0963 ns |  1.00 | 0.0043 |      72 B |        1.00 |
| Instance_BindAsync        | True    | False |   7.739 ns | 0.0655 ns | 0.0613 ns |  0.48 | 0.0048 |      80 B |        1.11 |
| Instance_Bind_Obsolete    | True    | False |   7.732 ns | 0.0749 ns | 0.0701 ns |  0.48 | 0.0048 |      80 B |        1.11 |
|                           |         |       |            |           |           |       |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **True**  | **834.686 ns** | **4.8352 ns** | **4.5229 ns** |  **1.00** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | True  | 839.333 ns | 4.7165 ns | 4.4118 ns |  1.01 | 0.0134 |     239 B |        1.00 |
| Instance_BindAsync        | True    | True  | 804.374 ns | 6.4447 ns | 6.0283 ns |  0.96 | 0.0057 |     104 B |        0.44 |
| Instance_Bind_Obsolete    | True    | True  | 796.856 ns | 6.1604 ns | 5.7624 ns |  0.95 | 0.0057 |     104 B |        0.44 |
