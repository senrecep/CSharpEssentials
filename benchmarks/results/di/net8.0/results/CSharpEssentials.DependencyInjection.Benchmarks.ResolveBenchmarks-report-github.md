```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                | Categories | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| DecoratedManual       | Decorated  | 150.1 ns | 0.75 ns | 0.70 ns |  1.00 | 0.0215 |     360 B |        1.00 |
| DecoratedCseGenerated | Decorated  | 191.7 ns | 0.89 ns | 0.83 ns |  1.28 | 0.0215 |     360 B |        1.00 |
| DecoratedCseRuntime   | Decorated  | 217.1 ns | 2.26 ns | 1.89 ns |  1.45 | 0.0215 |     360 B |        1.00 |
| DecoratedScrutor      | Decorated  | 349.5 ns | 1.47 ns | 1.30 ns |  2.33 | 0.0234 |     392 B |        1.09 |
|                       |            |          |         |         |       |        |           |             |
| PlainManual           | Plain      | 153.5 ns | 0.54 ns | 0.48 ns |  1.00 | 0.0215 |     360 B |        1.00 |
| PlainCseGenerated     | Plain      | 154.0 ns | 0.71 ns | 0.67 ns |  1.00 | 0.0215 |     360 B |        1.00 |
| PlainScrutor          | Plain      | 154.2 ns | 0.45 ns | 0.35 ns |  1.00 | 0.0215 |     360 B |        1.00 |
