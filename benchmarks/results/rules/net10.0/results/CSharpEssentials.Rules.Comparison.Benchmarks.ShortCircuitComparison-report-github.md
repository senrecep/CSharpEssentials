```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean       | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-----------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.355 ns |  0.1062 ns |  0.0993 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  16.269 ns |  0.1133 ns |  0.0946 ns |  1.13 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  44.680 ns |  0.1463 ns |  0.1368 ns |  3.11 |    0.02 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   | 120.999 ns |  0.6278 ns |  0.5872 ns |  8.43 |    0.07 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 956.349 ns | 10.6291 ns |  9.4224 ns | 66.63 |    0.77 | 0.1278 |    2160 B |       38.57 |
| HandWrittenOrAll |   6.887 ns |  0.0042 ns |  0.0033 ns |  0.48 |    0.00 |      - |         - |        0.00 |
| CseOrAllFail     | 739.778 ns | 11.3139 ns | 10.5830 ns | 51.54 |    0.79 | 0.1135 |    1904 B |       34.00 |
