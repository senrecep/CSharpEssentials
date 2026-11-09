```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  16.77 ns | 0.129 ns | 0.121 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  37.63 ns | 0.160 ns | 0.142 ns |  2.24 |    0.02 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  50.85 ns | 0.228 ns | 0.214 ns |  3.03 |    0.02 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |  49.23 ns | 0.239 ns | 0.223 ns |  2.94 |    0.02 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 500.87 ns | 4.794 ns | 4.484 ns | 29.87 |    0.33 | 0.0029 |      56 B |        1.00 |
| HandWrittenOrAll |  13.24 ns | 0.028 ns | 0.025 ns |  0.79 |    0.01 |      - |         - |        0.00 |
| CseOrAllFail     | 479.80 ns | 2.109 ns | 1.761 ns | 28.61 |    0.22 | 0.0534 |     904 B |       16.14 |
