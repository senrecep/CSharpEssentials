```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  11.110 ns | 0.0735 ns | 0.0651 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  13.731 ns | 0.0583 ns | 0.0517 ns |  1.24 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  43.329 ns | 0.5828 ns | 0.5166 ns |  3.90 |    0.05 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 109.613 ns | 0.2916 ns | 0.2727 ns |  9.87 |    0.06 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 418.706 ns | 2.6350 ns | 2.4647 ns | 37.69 |    0.30 | 0.0033 |      56 B |        1.00 |
| HandWrittenOrAll |   5.417 ns | 0.0034 ns | 0.0032 ns |  0.49 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 757.887 ns | 4.2949 ns | 4.0174 ns | 68.22 |    0.52 | 0.0534 |     904 B |       16.14 |
