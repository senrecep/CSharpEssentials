```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 126.8 ns | 1.80 ns | 1.69 ns |  1.00 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 190.6 ns | 1.28 ns | 1.13 ns |  1.50 |    0.02 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 220.3 ns | 2.44 ns | 2.28 ns |  1.74 |    0.03 | 0.0234 |     392 B |        1.09 |
| DecoratedScrutor      | Decorated  | 350.0 ns | 3.37 ns | 2.99 ns |  2.76 |    0.04 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 146.4 ns | 1.13 ns | 1.05 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 147.3 ns | 1.56 ns | 1.46 ns |  1.01 |    0.01 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 150.9 ns | 1.18 ns | 1.05 ns |  1.03 |    0.01 | 0.0215 |     360 B |        1.00 |
