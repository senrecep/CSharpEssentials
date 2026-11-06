```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_AppendToUri | AppendToUri |   323.3 ns |  1.10 ns |  0.98 ns |  1.00 |    0.00 | 0.0467 |     784 B |        1.00 |
| AspNetCore_AppendToUri       | AppendToUri |   287.1 ns |  0.84 ns |  0.70 ns |  0.89 |    0.00 | 0.0391 |     656 B |        0.84 |
| Flurl_AppendToUri            | AppendToUri | 3,150.6 ns | 15.88 ns | 13.26 ns |  9.75 |    0.05 | 0.3662 |    6448 B |        8.22 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Dictionary  | Dictionary  |   183.1 ns |  0.98 ns |  0.87 ns |  1.00 |    0.01 | 0.0324 |     544 B |        1.00 |
| AspNetCore_QueryHelpers      | Dictionary  |   199.8 ns |  1.31 ns |  1.22 ns |  1.09 |    0.01 | 0.0324 |     544 B |        1.00 |
| Flurl_Dictionary             | Dictionary  | 2,223.0 ns | 10.43 ns |  9.76 ns | 12.14 |    0.08 | 0.2785 |    4696 B |        8.63 |
| Bcl_Dictionary               | Dictionary  |   172.1 ns |  1.18 ns |  1.10 ns |  0.94 |    0.01 | 0.0291 |     488 B |        0.90 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Object      | Object      |   363.8 ns |  1.85 ns |  1.73 ns |  1.00 |    0.01 | 0.0482 |     808 B |        1.00 |
| Flurl_Object                 | Object      | 2,289.6 ns | 11.92 ns |  9.95 ns |  6.29 |    0.04 | 0.3242 |    5432 B |        6.72 |
