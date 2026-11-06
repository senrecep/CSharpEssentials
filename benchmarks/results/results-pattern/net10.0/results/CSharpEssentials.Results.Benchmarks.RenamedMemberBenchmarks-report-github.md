```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                    | Succeed | Yield | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_MatchAsync**     | **False**   | **False** |  **11.243 ns** | **0.0100 ns** | **0.0089 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_Match_Obsolete | False   | False |  11.552 ns | 0.0276 ns | 0.0245 ns |  1.03 |    0.00 |      - |         - |          NA |
| Instance_BindAsync        | False   | False |  22.408 ns | 0.2484 ns | 0.2323 ns |  1.99 |    0.02 | 0.0081 |     136 B |          NA |
| Instance_Bind_Obsolete    | False   | False |  22.773 ns | 0.3140 ns | 0.2937 ns |  2.03 |    0.03 | 0.0081 |     136 B |          NA |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **False**   | **True**  | **849.127 ns** | **4.2097 ns** | **3.9378 ns** |  **1.00** |    **0.01** | **0.0172** |     **295 B** |        **1.00** |
| TaskSource_Match_Obsolete | False   | True  | 851.176 ns | 5.0607 ns | 4.7338 ns |  1.00 |    0.01 | 0.0172 |     295 B |        1.00 |
| Instance_BindAsync        | False   | True  |  22.010 ns | 0.1835 ns | 0.1717 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
| Instance_Bind_Obsolete    | False   | True  |  22.975 ns | 0.2211 ns | 0.2068 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **False** |  **16.568 ns** | **0.0987 ns** | **0.0923 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | False |  17.220 ns | 0.0919 ns | 0.0767 ns |  1.04 |    0.01 | 0.0043 |      72 B |        1.00 |
| Instance_BindAsync        | True    | False |   8.114 ns | 0.1463 ns | 0.1369 ns |  0.49 |    0.01 | 0.0048 |      80 B |        1.11 |
| Instance_Bind_Obsolete    | True    | False |   8.130 ns | 0.0807 ns | 0.0754 ns |  0.49 |    0.01 | 0.0048 |      80 B |        1.11 |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **True**  | **833.145 ns** | **5.7212 ns** | **5.3516 ns** |  **1.00** |    **0.01** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | True  | 841.911 ns | 2.4931 ns | 2.2101 ns |  1.01 |    0.01 | 0.0134 |     239 B |        1.00 |
| Instance_BindAsync        | True    | True  | 780.767 ns | 3.1825 ns | 2.9769 ns |  0.94 |    0.01 | 0.0057 |     104 B |        0.44 |
| Instance_Bind_Obsolete    | True    | True  | 790.941 ns | 3.3854 ns | 3.0011 ns |  0.95 |    0.01 | 0.0057 |     104 B |        0.44 |
