```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |    15.70 ns | 0.102 ns | 0.096 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |    35.19 ns | 0.122 ns | 0.114 ns |  2.24 |    0.02 | 0.0033 |      56 B |        1.00 |
| CseAnd           |    69.64 ns | 0.144 ns | 0.135 ns |  4.44 |    0.03 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |    51.13 ns | 0.374 ns | 0.350 ns |  3.26 |    0.03 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 1,103.53 ns | 4.504 ns | 3.993 ns | 70.30 |    0.48 | 0.1278 |    2160 B |       38.57 |
| HandWrittenOrAll |    12.08 ns | 0.027 ns | 0.026 ns |  0.77 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     |   895.12 ns | 5.013 ns | 4.689 ns | 57.02 |    0.44 | 0.1135 |    1904 B |       34.00 |
