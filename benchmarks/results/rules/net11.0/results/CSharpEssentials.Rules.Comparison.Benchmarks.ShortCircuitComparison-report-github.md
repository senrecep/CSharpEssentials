```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  11.434 ns | 0.0752 ns | 0.0704 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  13.841 ns | 0.1113 ns | 0.0986 ns |  1.21 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  17.011 ns | 0.1280 ns | 0.1198 ns |  1.49 |    0.01 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 110.819 ns | 0.4230 ns | 0.3750 ns |  9.69 |    0.07 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 399.326 ns | 1.5109 ns | 1.4133 ns | 34.92 |    0.24 | 0.0033 |      56 B |        1.00 |
| HandWrittenOrAll |   5.424 ns | 0.0064 ns | 0.0050 ns |  0.47 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 375.735 ns | 5.0255 ns | 4.7009 ns | 32.86 |    0.44 | 0.0539 |     904 B |       16.14 |
