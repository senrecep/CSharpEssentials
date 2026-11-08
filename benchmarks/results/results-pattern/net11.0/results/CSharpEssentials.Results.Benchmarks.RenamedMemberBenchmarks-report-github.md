```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                    | Succeed | Yield | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_MatchAsync**     | **False**   | **False** |  **11.177 ns** | **0.0048 ns** | **0.0045 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_Match_Obsolete | False   | False |  11.139 ns | 0.0045 ns | 0.0042 ns |  1.00 |    0.00 |      - |         - |          NA |
| Instance_BindAsync        | False   | False |   8.651 ns | 0.1210 ns | 0.1132 ns |  0.77 |    0.01 | 0.0048 |      80 B |          NA |
| Instance_Bind_Obsolete    | False   | False |   8.488 ns | 0.1900 ns | 0.1777 ns |  0.76 |    0.02 | 0.0048 |      80 B |          NA |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **False**   | **True**  | **550.965 ns** | **5.6685 ns** | **5.3023 ns** |  **1.00** |    **0.01** | **0.0134** |     **237 B** |        **1.00** |
| TaskSource_Match_Obsolete | False   | True  | 551.894 ns | 4.8249 ns | 4.5132 ns |  1.00 |    0.01 | 0.0134 |     236 B |        1.00 |
| Instance_BindAsync        | False   | True  |   9.010 ns | 0.0954 ns | 0.0892 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.34 |
| Instance_Bind_Obsolete    | False   | True  |   8.635 ns | 0.1013 ns | 0.0947 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.34 |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **False** |  **16.161 ns** | **0.1240 ns** | **0.1159 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | False |  15.557 ns | 0.2877 ns | 0.2691 ns |  0.96 |    0.02 | 0.0043 |      72 B |        1.00 |
| Instance_BindAsync        | True    | False |   7.864 ns | 0.1182 ns | 0.1106 ns |  0.49 |    0.01 | 0.0048 |      80 B |        1.11 |
| Instance_Bind_Obsolete    | True    | False |   6.815 ns | 0.0589 ns | 0.0551 ns |  0.42 |    0.00 | 0.0048 |      80 B |        1.11 |
|                           |         |       |            |           |           |       |         |        |           |             |
| **TaskSource_MatchAsync**     | **True**    | **True**  | **556.277 ns** | **8.4080 ns** | **7.8649 ns** |  **1.00** |    **0.02** | **0.0134** |     **238 B** |        **1.00** |
| TaskSource_Match_Obsolete | True    | True  | 556.466 ns | 5.5787 ns | 5.2183 ns |  1.00 |    0.02 | 0.0134 |     238 B |        1.00 |
| Instance_BindAsync        | True    | True  | 492.146 ns | 7.3830 ns | 6.9060 ns |  0.88 |    0.02 | 0.0062 |     104 B |        0.44 |
| Instance_Bind_Obsolete    | True    | True  | 467.961 ns | 7.4066 ns | 6.9282 ns |  0.84 |    0.02 | 0.0062 |     104 B |        0.44 |
