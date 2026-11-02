```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 123.5 ns | 1.08 ns | 1.01 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 178.5 ns | 0.93 ns | 0.87 ns |  1.45 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 213.7 ns | 1.68 ns | 1.57 ns |  1.73 |    0.02 | 0.0234 |     392 B |        1.09 |
| DecoratedScrutor      | Decorated  | 329.0 ns | 1.34 ns | 1.25 ns |  2.67 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 147.7 ns | 0.58 ns | 0.52 ns |  1.00 |    0.00 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 156.5 ns | 0.96 ns | 0.89 ns |  1.06 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 146.7 ns | 1.15 ns | 1.08 ns |  0.99 |    0.01 | 0.0215 |     360 B |        1.00 |
