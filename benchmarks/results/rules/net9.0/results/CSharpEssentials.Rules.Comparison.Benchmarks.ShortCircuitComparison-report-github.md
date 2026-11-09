```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.29 ns | 0.072 ns | 0.060 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  34.89 ns | 0.147 ns | 0.138 ns |  2.44 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  41.41 ns | 0.221 ns | 0.206 ns |  2.90 |    0.02 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |  50.90 ns | 0.265 ns | 0.248 ns |  3.56 |    0.02 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 439.43 ns | 0.825 ns | 0.731 ns | 30.75 |    0.13 | 0.0033 |      56 B |        1.00 |
| HandWrittenOrAll |  11.92 ns | 0.026 ns | 0.023 ns |  0.83 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 446.61 ns | 6.490 ns | 6.071 ns | 31.26 |    0.43 | 0.0539 |     904 B |       16.14 |
