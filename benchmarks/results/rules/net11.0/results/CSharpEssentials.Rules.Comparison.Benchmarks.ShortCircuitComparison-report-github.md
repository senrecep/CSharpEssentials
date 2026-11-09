```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  11.180 ns | 0.0734 ns | 0.0686 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  13.834 ns | 0.1266 ns | 0.1184 ns |  1.24 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  17.116 ns | 0.1283 ns | 0.1200 ns |  1.53 |    0.01 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 112.736 ns | 0.2679 ns | 0.2506 ns | 10.08 |    0.06 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 418.481 ns | 3.3605 ns | 3.1434 ns | 37.43 |    0.35 | 0.0033 |      56 B |        1.00 |
| HandWrittenOrAll |   5.426 ns | 0.0091 ns | 0.0085 ns |  0.49 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 388.191 ns | 1.4925 ns | 1.3961 ns | 34.72 |    0.24 | 0.0539 |     904 B |       16.14 |
