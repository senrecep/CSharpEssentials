```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.58 ns | 0.051 ns | 0.045 ns |  1.00 |    0.00 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  34.62 ns | 0.182 ns | 0.152 ns |  2.37 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  72.17 ns | 1.460 ns | 1.738 ns |  4.95 |    0.12 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |  49.32 ns | 0.204 ns | 0.191 ns |  3.38 |    0.02 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 530.90 ns | 1.939 ns | 1.813 ns | 36.42 |    0.16 | 0.0029 |      56 B |        1.00 |
| HandWrittenOrAll |  12.33 ns | 0.041 ns | 0.036 ns |  0.85 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 756.52 ns | 9.304 ns | 8.703 ns | 51.89 |    0.60 | 0.0534 |     904 B |       16.14 |
