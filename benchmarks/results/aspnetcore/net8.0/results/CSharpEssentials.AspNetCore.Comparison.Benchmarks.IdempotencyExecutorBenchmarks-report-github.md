```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |     8.010 ns | 0.0082 ns | 0.0077 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |    46.113 ns | 0.0589 ns | 0.0551 ns |  5.76 |    0.01 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    |   664.826 ns | 7.0372 ns | 6.5826 ns | 83.00 |    0.80 | 0.0296 | 0.0238 |     504 B |          NA |
| CSharpEssentials_Replay          | Execute    |   161.939 ns | 0.2720 ns | 0.2545 ns | 20.22 |    0.04 |      - |      - |         - |          NA |
| CSharpEssentials_InFlight        | Execute    |   147.843 ns | 0.2656 ns | 0.2485 ns | 18.46 |    0.03 |      - |      - |         - |          NA |
| CSharpEssentials_KeyReused       | Execute    |   151.747 ns | 0.6446 ns | 0.6030 ns | 18.94 |    0.07 |      - |      - |         - |          NA |
| CSharpEssentials_ModeOff         | Execute    |    74.582 ns | 0.2323 ns | 0.2173 ns |  9.31 |    0.03 |      - |      - |         - |          NA |
|                                  |            |              |           |           |       |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 1,121.019 ns | 6.8852 ns | 6.1036 ns |  1.00 |    0.01 | 0.0610 | 0.0534 |    1040 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      |   480.771 ns | 5.1681 ns | 4.8342 ns |  0.43 |    0.00 | 0.0339 |      - |     568 B |        0.55 |
