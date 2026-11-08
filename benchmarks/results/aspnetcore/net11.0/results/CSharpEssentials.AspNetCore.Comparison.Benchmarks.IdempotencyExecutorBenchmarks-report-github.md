```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-----------:|----------:|----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |   1.049 ns | 0.0027 ns | 0.0025 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |  12.722 ns | 0.0120 ns | 0.0106 ns |  12.13 |    0.03 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    | 545.730 ns | 4.7639 ns | 4.4562 ns | 520.26 |    4.28 | 0.0296 | 0.0238 |     504 B |          NA |
| CSharpEssentials_Replay          | Execute    | 121.636 ns | 0.2727 ns | 0.2551 ns | 115.96 |    0.36 |      - |      - |         - |          NA |
| CSharpEssentials_InFlight        | Execute    | 120.203 ns | 0.1191 ns | 0.1114 ns | 114.59 |    0.28 |      - |      - |         - |          NA |
| CSharpEssentials_KeyReused       | Execute    | 116.737 ns | 0.4134 ns | 0.3867 ns | 111.29 |    0.44 |      - |      - |         - |          NA |
| CSharpEssentials_ModeOff         | Execute    |  53.385 ns | 0.0610 ns | 0.0571 ns |  50.89 |    0.13 |      - |      - |         - |          NA |
|                                  |            |            |           |           |        |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 953.367 ns | 4.4512 ns | 4.1636 ns |   1.00 |    0.01 | 0.0591 | 0.0486 |    1000 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      | 394.532 ns | 1.8834 ns | 1.7617 ns |   0.41 |    0.00 | 0.0315 |      - |     528 B |        0.53 |
