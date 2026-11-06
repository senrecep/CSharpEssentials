```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                    | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_MatchAsync**     | **False**   | **False** |    **11.570 ns** |  **0.0053 ns** |  **0.0049 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_Match_Obsolete | False   | False |    11.757 ns |  0.0341 ns |  0.0302 ns |  1.02 |    0.00 |      - |         - |          NA |
| Instance_BindAsync        | False   | False |    21.788 ns |  0.3560 ns |  0.3330 ns |  1.88 |    0.03 | 0.0081 |     136 B |          NA |
| Instance_Bind_Obsolete    | False   | False |    21.448 ns |  0.3040 ns |  0.2843 ns |  1.85 |    0.02 | 0.0081 |     136 B |          NA |
|                           |         |       |              |            |            |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **False**   | **True**  | **3,249.552 ns** | **17.2569 ns** | **15.2978 ns** | **1.000** |    **0.01** | **0.0153** |     **296 B** |        **1.00** |
| TaskSource_Match_Obsolete | False   | True  | 3,238.354 ns | 34.9288 ns | 32.6724 ns | 0.997 |    0.01 | 0.0153 |     296 B |        1.00 |
| Instance_BindAsync        | False   | True  |    23.061 ns |  0.4852 ns |  0.5393 ns | 0.007 |    0.00 | 0.0081 |     136 B |        0.46 |
| Instance_Bind_Obsolete    | False   | True  |    21.727 ns |  0.3239 ns |  0.2871 ns | 0.007 |    0.00 | 0.0081 |     136 B |        0.46 |
|                           |         |       |              |            |            |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **False** |    **17.836 ns** |  **0.1398 ns** |  **0.1308 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | False |    16.307 ns |  0.1138 ns |  0.1065 ns |  0.91 |    0.01 | 0.0043 |      72 B |        1.00 |
| Instance_BindAsync        | True    | False |     7.879 ns |  0.1369 ns |  0.1281 ns |  0.44 |    0.01 | 0.0048 |      80 B |        1.11 |
| Instance_Bind_Obsolete    | True    | False |     7.950 ns |  0.1338 ns |  0.1251 ns |  0.45 |    0.01 | 0.0048 |      80 B |        1.11 |
|                           |         |       |              |            |            |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **True**  | **3,246.700 ns** | **24.5918 ns** | **23.0032 ns** |  **1.00** |    **0.01** | **0.0114** |     **240 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | True  | 3,207.661 ns | 33.0090 ns | 30.8766 ns |  0.99 |    0.01 | 0.0114 |     240 B |        1.00 |
| Instance_BindAsync        | True    | True  | 3,169.522 ns | 27.0937 ns | 24.0178 ns |  0.98 |    0.01 | 0.0038 |     104 B |        0.43 |
| Instance_Bind_Obsolete    | True    | True  | 3,189.548 ns | 30.6679 ns | 28.6868 ns |  0.98 |    0.01 | 0.0038 |     104 B |        0.43 |
