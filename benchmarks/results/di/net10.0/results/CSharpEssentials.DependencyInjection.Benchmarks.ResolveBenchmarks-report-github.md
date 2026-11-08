```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 111.0 ns | 0.71 ns | 0.63 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 129.7 ns | 0.95 ns | 0.89 ns |  1.17 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 138.1 ns | 0.54 ns | 0.48 ns |  1.24 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 391.7 ns | 1.06 ns | 0.99 ns |  3.53 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 137.9 ns | 0.66 ns | 0.59 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 137.8 ns | 2.06 ns | 1.92 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 136.4 ns | 1.64 ns | 1.54 ns |  0.99 |    0.01 | 0.0215 |     360 B |        1.00 |
