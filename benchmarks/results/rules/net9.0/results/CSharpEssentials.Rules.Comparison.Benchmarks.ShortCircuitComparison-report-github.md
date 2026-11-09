```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten      |  14.31 ns | 0.116 ns | 0.102 ns |  1.00 |    0.01 | 0.0033 |      56 B |        1.00 |
| CseLinear        |  34.50 ns | 0.218 ns | 0.193 ns |  2.41 |    0.02 | 0.0033 |      56 B |        1.00 |
| CseAnd           |  42.09 ns | 0.229 ns | 0.214 ns |  2.94 |    0.02 | 0.0033 |      56 B |        1.00 |
| HandWrittenAll   |  50.62 ns | 0.278 ns | 0.260 ns |  3.54 |    0.03 | 0.0086 |     144 B |        2.57 |
| CseAndGeneric    | 445.02 ns | 0.876 ns | 0.777 ns | 31.10 |    0.22 | 0.0033 |      56 B |        1.00 |
| HandWrittenOrAll |  11.88 ns | 0.014 ns | 0.013 ns |  0.83 |    0.01 |      - |         - |        0.00 |
| CseOrAllFail     | 450.05 ns | 2.125 ns | 1.884 ns | 31.45 |    0.25 | 0.0539 |     904 B |       16.14 |
