```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 110.0 ns | 0.64 ns | 0.60 ns |  1.00 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 122.1 ns | 0.69 ns | 0.65 ns |  1.11 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 136.3 ns | 0.79 ns | 0.73 ns |  1.24 |    0.01 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 320.0 ns | 0.86 ns | 0.80 ns |  2.91 |    0.02 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |         |        |           |             |
| PlainManual           | Plain      | 142.7 ns | 0.55 ns | 0.51 ns |  1.00 |    0.00 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 142.7 ns | 0.54 ns | 0.50 ns |  1.00 |    0.00 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 141.8 ns | 0.83 ns | 0.78 ns |  0.99 |    0.01 | 0.0215 |     360 B |        1.00 |
