```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                    | Succeed | Yield | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_MatchAsync**     | **False**   | **False** |  **13.045 ns** | **0.0272 ns** | **0.0254 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_Match_Obsolete | False   | False |  13.610 ns | 0.0156 ns | 0.0130 ns |  1.04 |    0.00 |      - |         - |          NA |
| Instance_BindAsync        | False   | False |  28.535 ns | 0.5521 ns | 0.5164 ns |  2.19 |    0.04 | 0.0081 |     136 B |          NA |
| Instance_Bind_Obsolete    | False   | False |  32.540 ns | 0.2271 ns | 0.2013 ns |  2.49 |    0.02 | 0.0081 |     136 B |          NA |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **False**   | **True**  | **859.768 ns** | **6.0023 ns** | **5.6145 ns** |  **1.00** |    **0.01** | **0.0172** |     **295 B** |        **1.00** |
| TaskSource_Match_Obsolete | False   | True  | 861.187 ns | 3.9398 ns | 3.6853 ns |  1.00 |    0.01 | 0.0172 |     295 B |        1.00 |
| Instance_BindAsync        | False   | True  |  28.440 ns | 0.5183 ns | 0.4848 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
| Instance_Bind_Obsolete    | False   | True  |  28.689 ns | 0.5937 ns | 0.5831 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **False** |  **18.931 ns** | **0.1150 ns** | **0.1076 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | False |  18.713 ns | 0.1140 ns | 0.1011 ns |  0.99 |    0.01 | 0.0043 |      72 B |        1.00 |
| Instance_BindAsync        | True    | False |  10.043 ns | 0.2235 ns | 0.2091 ns |  0.53 |    0.01 | 0.0048 |      80 B |        1.11 |
| Instance_Bind_Obsolete    | True    | False |   9.769 ns | 0.0904 ns | 0.0845 ns |  0.52 |    0.01 | 0.0048 |      80 B |        1.11 |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **True**  | **850.553 ns** | **4.8607 ns** | **4.5467 ns** |  **1.00** |    **0.01** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | True  | 851.154 ns | 3.7310 ns | 3.4900 ns |  1.00 |    0.01 | 0.0134 |     239 B |        1.00 |
| Instance_BindAsync        | True    | True  | 787.872 ns | 2.5859 ns | 2.4189 ns |  0.93 |    0.01 | 0.0057 |     104 B |        0.44 |
| Instance_Bind_Obsolete    | True    | True  | 784.242 ns | 4.8221 ns | 4.5106 ns |  0.92 |    0.01 | 0.0057 |     104 B |        0.44 |
