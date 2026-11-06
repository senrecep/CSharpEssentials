```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 115.2 ns | 1.66 ns | 1.55 ns |  1.00 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 162.5 ns | 0.78 ns | 0.73 ns |  1.41 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 178.8 ns | 0.75 ns | 0.70 ns |  1.55 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 323.0 ns | 0.86 ns | 0.81 ns |  2.80 |    0.04 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 137.9 ns | 0.59 ns | 0.55 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 147.6 ns | 1.19 ns | 1.11 ns |  1.07 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 145.8 ns | 0.67 ns | 0.62 ns |  1.06 |    0.01 | 0.0215 |     360 B |        1.00 |
