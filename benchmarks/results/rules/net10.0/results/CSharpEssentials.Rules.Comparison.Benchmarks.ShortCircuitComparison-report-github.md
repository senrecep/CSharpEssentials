```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.149 ns | 0.1066 ns | 0.0945 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  16.193 ns | 0.1590 ns | 0.1487 ns |  1.14 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  20.415 ns | 0.0982 ns | 0.0918 ns |  1.44 |    0.01 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 117.368 ns | 0.5565 ns | 0.5205 ns |  8.30 |    0.06 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 429.854 ns | 2.4790 ns | 2.3189 ns | 30.38 |    0.25 | 0.0029 |      56 B |        1.00 |
| HandWrittenOrAll |   6.910 ns | 0.0141 ns | 0.0132 ns |  0.49 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 392.511 ns | 5.0010 ns | 4.6779 ns | 27.74 |    0.37 | 0.0539 |     904 B |       16.14 |
