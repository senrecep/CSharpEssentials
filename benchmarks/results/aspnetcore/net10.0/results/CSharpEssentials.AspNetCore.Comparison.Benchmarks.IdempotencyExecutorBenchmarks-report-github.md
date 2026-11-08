```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean       | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-----------:|-----------:|-----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |   1.418 ns |  0.0098 ns |  0.0091 ns |   1.00 |    0.01 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |  14.194 ns |  0.0245 ns |  0.0229 ns |  10.01 |    0.06 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    | 549.333 ns |  4.2837 ns |  4.0069 ns | 387.41 |    3.65 | 0.0296 | 0.0238 |     504 B |          NA |
| CSharpEssentials_Replay          | Execute    | 119.891 ns |  0.2947 ns |  0.2756 ns |  84.55 |    0.56 |      - |      - |         - |          NA |
| CSharpEssentials_InFlight        | Execute    | 117.577 ns |  0.3118 ns |  0.2916 ns |  82.92 |    0.55 |      - |      - |         - |          NA |
| CSharpEssentials_KeyReused       | Execute    | 112.699 ns |  0.1801 ns |  0.1596 ns |  79.48 |    0.51 |      - |      - |         - |          NA |
| CSharpEssentials_ModeOff         | Execute    |  51.513 ns |  0.0662 ns |  0.0587 ns |  36.33 |    0.23 |      - |      - |         - |          NA |
|                                  |            |            |            |            |        |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 958.428 ns | 10.7661 ns | 10.0706 ns |   1.00 |    0.01 | 0.0591 | 0.0477 |    1000 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      | 380.314 ns |  1.0714 ns |  0.9498 ns |   0.40 |    0.00 | 0.0315 |      - |     528 B |        0.53 |
