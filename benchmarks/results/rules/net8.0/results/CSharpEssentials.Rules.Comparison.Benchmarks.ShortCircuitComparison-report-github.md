```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  16.86 ns | 0.097 ns | 0.091 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  38.12 ns | 0.185 ns | 0.173 ns |  2.26 |    0.02 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  50.42 ns | 0.152 ns | 0.142 ns |  2.99 |    0.02 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |  49.83 ns | 0.164 ns | 0.154 ns |  2.96 |    0.02 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 531.32 ns | 1.438 ns | 1.345 ns | 31.52 |    0.18 | 0.0029 |      56 B |        1.00 |
| HandWrittenOrAll |  13.46 ns | 0.032 ns | 0.030 ns |  0.80 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 483.47 ns | 3.399 ns | 3.179 ns | 28.68 |    0.24 | 0.0534 |     904 B |       16.14 |
