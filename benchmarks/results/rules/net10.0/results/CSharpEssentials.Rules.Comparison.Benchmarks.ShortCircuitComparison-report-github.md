```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.136 ns | 0.0954 ns | 0.0797 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  16.365 ns | 0.0620 ns | 0.0518 ns |  1.16 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  46.832 ns | 0.8645 ns | 0.7664 ns |  3.31 |    0.06 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 120.233 ns | 0.8031 ns | 0.7512 ns |  8.51 |    0.07 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 425.224 ns | 0.5222 ns | 0.4884 ns | 30.08 |    0.17 | 0.0029 |      56 B |        1.00 |
| HandWrittenOrAll |   6.888 ns | 0.0028 ns | 0.0026 ns |  0.49 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 715.255 ns | 6.9148 ns | 6.4682 ns | 50.60 |    0.52 | 0.0534 |     904 B |       16.14 |
