```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 114.6 ns | 0.82 ns | 0.77 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 193.8 ns | 2.12 ns | 1.98 ns |  1.69 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 184.1 ns | 0.73 ns | 0.61 ns |  1.61 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 321.2 ns | 2.63 ns | 2.46 ns |  2.80 |    0.03 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 148.5 ns | 1.28 ns | 1.20 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 147.7 ns | 1.28 ns | 1.20 ns |  0.99 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 144.9 ns | 0.59 ns | 0.55 ns |  0.98 |    0.01 | 0.0215 |     360 B |        1.00 |
