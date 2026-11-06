```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean         | Error      | StdDev     | Ratio    | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-------------:|-----------:|-----------:|---------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |     1.424 ns |  0.0138 ns |  0.0129 ns |     1.00 |    0.01 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |    15.138 ns |  0.0197 ns |  0.0184 ns |    10.63 |    0.09 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    | 1,486.117 ns | 11.4068 ns | 10.6700 ns | 1,043.78 |   11.71 | 0.0343 | 0.0305 |     592 B |          NA |
| CSharpEssentials_Replay          | Execute    |   130.799 ns |  0.5063 ns |  0.4736 ns |    91.87 |    0.87 | 0.0091 |      - |     152 B |          NA |
| CSharpEssentials_InFlight        | Execute    |   117.662 ns |  0.3616 ns |  0.3383 ns |    82.64 |    0.76 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_KeyReused       | Execute    |   121.813 ns |  0.1942 ns |  0.1722 ns |    85.56 |    0.76 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_ModeOff         | Execute    |    59.187 ns |  0.1490 ns |  0.1393 ns |    41.57 |    0.38 | 0.0057 |      - |      96 B |          NA |
|                                  |            |              |            |            |          |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 1,926.299 ns |  9.5303 ns |  8.4484 ns |     1.00 |    0.01 | 0.0648 | 0.0420 |    1112 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      |   418.617 ns |  0.4773 ns |  0.4464 ns |     0.22 |    0.00 | 0.0420 |      - |     704 B |        0.63 |
