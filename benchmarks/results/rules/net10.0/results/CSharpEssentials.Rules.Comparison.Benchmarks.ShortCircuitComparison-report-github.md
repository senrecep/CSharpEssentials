```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.069 ns | 0.0660 ns | 0.0516 ns |  1.00 |    0.00 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  19.523 ns | 0.0758 ns | 0.0709 ns |  1.39 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  20.034 ns | 0.0856 ns | 0.0800 ns |  1.42 |    0.01 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 117.954 ns | 0.7711 ns | 0.7213 ns |  8.38 |    0.06 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 436.086 ns | 1.0679 ns | 0.9989 ns | 31.00 |    0.13 | 0.0033 |      56 B |        1.00 |
| HandWrittenOrAll |   6.902 ns | 0.0134 ns | 0.0112 ns |  0.49 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 411.011 ns | 3.7865 ns | 3.3566 ns | 29.21 |    0.25 | 0.0539 |     904 B |       16.14 |
