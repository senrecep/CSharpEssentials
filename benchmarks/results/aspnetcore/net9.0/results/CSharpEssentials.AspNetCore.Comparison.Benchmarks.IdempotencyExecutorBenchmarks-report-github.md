```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean         | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-------------:|----------:|----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |     8.049 ns | 0.0314 ns | 0.0278 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |    23.431 ns | 0.0446 ns | 0.0395 ns |   2.91 |    0.01 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    | 1,566.874 ns | 4.1149 ns | 3.6478 ns | 194.67 |    0.78 | 0.0343 | 0.0305 |     592 B |          NA |
| CSharpEssentials_Replay          | Execute    |   181.050 ns | 0.3052 ns | 0.2549 ns |  22.49 |    0.08 | 0.0091 |      - |     152 B |          NA |
| CSharpEssentials_InFlight        | Execute    |   144.509 ns | 0.5610 ns | 0.4974 ns |  17.95 |    0.08 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_KeyReused       | Execute    |   143.841 ns | 0.1840 ns | 0.1631 ns |  17.87 |    0.06 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_ModeOff         | Execute    |    72.395 ns | 0.1993 ns | 0.1767 ns |   8.99 |    0.04 | 0.0057 |      - |      96 B |          NA |
|                                  |            |              |           |           |        |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 2,113.967 ns | 4.7664 ns | 4.4585 ns |   1.00 |    0.00 | 0.0687 | 0.0572 |    1152 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      |   448.081 ns | 1.1490 ns | 1.0186 ns |   0.21 |    0.00 | 0.0443 |      - |     744 B |        0.65 |
