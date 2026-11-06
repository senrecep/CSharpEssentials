```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |     7.973 ns |  0.0018 ns |  0.0016 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |    46.223 ns |  0.1908 ns |  0.1785 ns |   5.80 |    0.02 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    | 1,641.095 ns | 12.0431 ns | 11.2652 ns | 205.83 |    1.37 | 0.0343 | 0.0305 |     592 B |          NA |
| CSharpEssentials_Replay          | Execute    |   171.365 ns |  0.9462 ns |  0.8851 ns |  21.49 |    0.11 | 0.0091 |      - |     152 B |          NA |
| CSharpEssentials_InFlight        | Execute    |   151.481 ns |  0.2378 ns |  0.1985 ns |  19.00 |    0.02 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_KeyReused       | Execute    |   151.532 ns |  0.7468 ns |  0.6237 ns |  19.01 |    0.08 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_ModeOff         | Execute    |    74.482 ns |  0.3513 ns |  0.3286 ns |   9.34 |    0.04 | 0.0057 |      - |      96 B |          NA |
|                                  |            |              |            |            |        |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 2,222.718 ns | 12.8995 ns | 11.4351 ns |   1.00 |    0.01 | 0.0687 | 0.0572 |    1152 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      |   477.924 ns |  0.9743 ns |  0.8637 ns |   0.22 |    0.00 | 0.0439 |      - |     744 B |        0.65 |
