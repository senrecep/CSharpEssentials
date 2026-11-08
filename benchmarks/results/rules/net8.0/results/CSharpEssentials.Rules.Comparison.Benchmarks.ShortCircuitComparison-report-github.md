```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  16.16 ns | 0.085 ns | 0.080 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  38.45 ns | 0.135 ns | 0.126 ns |  2.38 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  79.81 ns | 0.262 ns | 0.245 ns |  4.94 |    0.03 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |  49.36 ns | 0.517 ns | 0.432 ns |  3.06 |    0.03 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 518.71 ns | 2.719 ns | 2.544 ns | 32.11 |    0.22 | 0.0029 |      56 B |        1.00 |
| HandWrittenOrAll |  13.17 ns | 0.008 ns | 0.008 ns |  0.82 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 827.20 ns | 8.090 ns | 7.172 ns | 51.20 |    0.49 | 0.0534 |     904 B |       16.14 |
