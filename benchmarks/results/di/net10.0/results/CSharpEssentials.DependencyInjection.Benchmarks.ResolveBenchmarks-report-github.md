```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 110.1 ns | 1.24 ns | 1.16 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 164.6 ns | 1.14 ns | 1.07 ns |  1.49 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 252.2 ns | 2.16 ns | 1.92 ns |  2.29 |    0.03 | 0.0234 |     392 B |        1.09 |
| DecoratedScrutor      | Decorated  | 309.6 ns | 1.82 ns | 1.70 ns |  2.81 |    0.03 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 132.6 ns | 0.97 ns | 0.91 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 134.5 ns | 1.01 ns | 0.95 ns |  1.01 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 134.4 ns | 1.50 ns | 1.40 ns |  1.01 |    0.01 | 0.0215 |     360 B |        1.00 |
