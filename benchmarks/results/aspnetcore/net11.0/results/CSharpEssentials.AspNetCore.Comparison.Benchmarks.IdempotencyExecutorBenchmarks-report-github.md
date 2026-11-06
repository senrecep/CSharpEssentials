```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                           | Categories | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------------- |----------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| Direct_Call                      | Execute    |     1.066 ns |  0.0159 ns |  0.0149 ns |   1.00 |    0.02 |      - |      - |         - |          NA |
| HandWritten_ConcurrentDictionary | Execute    |    12.380 ns |  0.0953 ns |  0.0891 ns |  11.62 |    0.18 |      - |      - |         - |          NA |
| CSharpEssentials_FirstRun        | Execute    |   603.325 ns |  3.0837 ns |  2.7336 ns | 566.26 |    7.98 | 0.0353 | 0.0315 |     592 B |          NA |
| CSharpEssentials_Replay          | Execute    |   129.085 ns |  0.3530 ns |  0.3302 ns | 121.15 |    1.65 | 0.0091 |      - |     152 B |          NA |
| CSharpEssentials_InFlight        | Execute    |   120.094 ns |  0.2194 ns |  0.1713 ns | 112.72 |    1.52 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_KeyReused       | Execute    |   124.695 ns |  1.3462 ns |  1.2592 ns | 117.03 |    1.94 | 0.0057 |      - |      96 B |          NA |
| CSharpEssentials_ModeOff         | Execute    |    58.841 ns |  0.6865 ns |  0.6085 ns |  55.23 |    0.92 | 0.0057 |      - |      96 B |          NA |
|                                  |            |              |            |            |        |         |        |        |           |             |
| CSharpEssentials_Inbox_New       | Inbox      | 1,027.703 ns | 11.6310 ns | 10.8796 ns |   1.00 |    0.01 | 0.0648 | 0.0477 |    1112 B |        1.00 |
| CSharpEssentials_Inbox_Duplicate | Inbox      |   415.982 ns |  1.5312 ns |  1.4323 ns |   0.40 |    0.00 | 0.0420 |      - |     704 B |        0.63 |
