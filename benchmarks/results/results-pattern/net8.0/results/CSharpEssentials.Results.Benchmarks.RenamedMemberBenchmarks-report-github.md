```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                    | Succeed | Yield | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-------- |------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_MatchAsync**     | **False**   | **False** |  **13.99 ns** | **0.020 ns** | **0.017 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_Match_Obsolete | False   | False |  13.75 ns | 0.009 ns | 0.007 ns |  0.98 |    0.00 |      - |         - |          NA |
| Instance_BindAsync        | False   | False |  30.12 ns | 0.618 ns | 0.578 ns |  2.15 |    0.04 | 0.0081 |     136 B |          NA |
| Instance_Bind_Obsolete    | False   | False |  29.70 ns | 0.351 ns | 0.311 ns |  2.12 |    0.02 | 0.0081 |     136 B |          NA |
|                           |         |       |           |          |          |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **False**   | **True**  | **862.02 ns** | **5.646 ns** | **5.281 ns** |  **1.00** |    **0.01** | **0.0172** |     **295 B** |        **1.00** |
| TaskSource_Match_Obsolete | False   | True  | 864.41 ns | 3.960 ns | 3.704 ns |  1.00 |    0.01 | 0.0172 |     295 B |        1.00 |
| Instance_BindAsync        | False   | True  |  29.61 ns | 0.487 ns | 0.456 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
| Instance_Bind_Obsolete    | False   | True  |  28.93 ns | 0.389 ns | 0.345 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
|                           |         |       |           |          |          |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **False** |  **19.27 ns** | **0.089 ns** | **0.079 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | False |  19.27 ns | 0.139 ns | 0.130 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| Instance_BindAsync        | True    | False |  10.10 ns | 0.115 ns | 0.107 ns |  0.52 |    0.01 | 0.0048 |      80 B |        1.11 |
| Instance_Bind_Obsolete    | True    | False |  10.10 ns | 0.157 ns | 0.147 ns |  0.52 |    0.01 | 0.0048 |      80 B |        1.11 |
|                           |         |       |           |          |          |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **True**  | **854.89 ns** | **4.496 ns** | **4.206 ns** |  **1.00** |    **0.01** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | True  | 859.31 ns | 7.192 ns | 6.727 ns |  1.01 |    0.01 | 0.0143 |     239 B |        1.00 |
| Instance_BindAsync        | True    | True  | 779.93 ns | 2.453 ns | 2.295 ns |  0.91 |    0.01 | 0.0057 |     104 B |        0.44 |
| Instance_Bind_Obsolete    | True    | True  | 783.91 ns | 3.994 ns | 3.736 ns |  0.92 |    0.01 | 0.0057 |     104 B |        0.44 |
