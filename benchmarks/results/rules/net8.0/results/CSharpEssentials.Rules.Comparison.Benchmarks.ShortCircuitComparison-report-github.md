```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |    15.79 ns |  0.333 ns |  0.456 ns |  1.00 |    0.04 | 0.0033 |      56 B |        1.00 |
| CseLinear        |    38.43 ns |  0.134 ns |  0.118 ns |  2.44 |    0.07 | 0.0033 |      56 B |        1.00 |
| CseAnd           |    80.23 ns |  0.162 ns |  0.151 ns |  5.08 |    0.14 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |    52.03 ns |  0.322 ns |  0.269 ns |  3.30 |    0.09 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 1,192.10 ns | 15.197 ns | 14.215 ns | 75.54 |    2.30 | 0.1278 |    2160 B |       38.57 |
| HandWrittenOrAll |    13.72 ns |  0.014 ns |  0.013 ns |  0.87 |    0.02 |      - |         - |        0.00 |
| CseOrAllFail     | 1,003.16 ns |  2.636 ns |  2.201 ns | 63.57 |    1.80 | 0.1125 |    1904 B |       34.00 |
