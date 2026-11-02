```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 109.8 ns | 0.55 ns | 0.51 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 185.6 ns | 0.71 ns | 0.67 ns |  1.69 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 203.0 ns | 1.62 ns | 1.51 ns |  1.85 |    0.02 | 0.0234 |     392 B |        1.09 |
| DecoratedScrutor      | Decorated  | 310.6 ns | 1.44 ns | 1.35 ns |  2.83 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 141.1 ns | 1.26 ns | 1.18 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 140.0 ns | 0.71 ns | 0.59 ns |  0.99 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 140.4 ns | 0.68 ns | 0.63 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
