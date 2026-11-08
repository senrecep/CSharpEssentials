```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 116.8 ns | 0.74 ns | 0.69 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 140.5 ns | 2.22 ns | 2.07 ns |  1.20 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 160.9 ns | 2.41 ns | 2.26 ns |  1.38 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 345.2 ns | 1.19 ns | 0.99 ns |  2.96 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 147.6 ns | 1.13 ns | 1.06 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 148.0 ns | 1.90 ns | 1.78 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 149.2 ns | 0.92 ns | 0.86 ns |  1.01 |    0.01 | 0.0215 |     360 B |        1.00 |
