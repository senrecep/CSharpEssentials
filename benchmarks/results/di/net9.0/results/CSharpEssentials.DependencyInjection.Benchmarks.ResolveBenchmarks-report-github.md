```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 127.1 ns | 0.94 ns | 0.88 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 193.4 ns | 1.95 ns | 1.82 ns |  1.52 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 196.4 ns | 1.00 ns | 0.94 ns |  1.55 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 337.5 ns | 1.97 ns | 1.84 ns |  2.66 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 158.2 ns | 1.10 ns | 1.03 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 153.7 ns | 0.57 ns | 0.47 ns |  0.97 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 155.8 ns | 0.52 ns | 0.48 ns |  0.98 |    0.01 | 0.0215 |     360 B |        1.00 |
