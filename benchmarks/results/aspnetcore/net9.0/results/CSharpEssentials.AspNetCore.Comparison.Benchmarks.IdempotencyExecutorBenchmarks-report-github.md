```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |     8.009 ns | 0.0125 ns | 0.0117 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |    24.603 ns | 0.0231 ns | 0.0180 ns |  3.07 |    0.00 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    |   610.364 ns | 5.7943 ns | 5.4200 ns | 76.21 |    0.66 | 0.0296 | 0.0238 |     504 B |          NA |
| CSharpEssentials_Replay          | Execute    |   162.398 ns | 0.8466 ns | 0.7505 ns | 20.28 |    0.09 |      - |      - |         - |          NA |
| CSharpEssentials_InFlight        | Execute    |   138.158 ns | 0.2802 ns | 0.2621 ns | 17.25 |    0.04 |      - |      - |         - |          NA |
| CSharpEssentials_KeyReused       | Execute    |   140.137 ns | 0.4107 ns | 0.3641 ns | 17.50 |    0.05 |      - |      - |         - |          NA |
| CSharpEssentials_ModeOff         | Execute    |    67.085 ns | 0.1140 ns | 0.1066 ns |  8.38 |    0.02 |      - |      - |         - |          NA |
|                                  |            |              |           |           |       |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 1,058.047 ns | 6.5008 ns | 6.0808 ns |  1.00 |    0.01 | 0.0610 | 0.0534 |    1040 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      |   434.117 ns | 1.8310 ns | 1.7127 ns |  0.41 |    0.00 | 0.0339 |      - |     568 B |        0.55 |
