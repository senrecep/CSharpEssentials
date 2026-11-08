```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 125.4 ns | 0.68 ns | 0.63 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 146.4 ns | 0.77 ns | 0.72 ns |  1.17 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 165.4 ns | 0.50 ns | 0.47 ns |  1.32 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 354.6 ns | 2.72 ns | 2.54 ns |  2.83 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 151.4 ns | 0.78 ns | 0.69 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 152.4 ns | 0.89 ns | 0.83 ns |  1.01 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 153.0 ns | 0.94 ns | 0.88 ns |  1.01 |    0.01 | 0.0215 |     360 B |        1.00 |
