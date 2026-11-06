```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_AppendToUri | AppendToUri |   374.5 ns |  2.93 ns |  2.74 ns |  1.00 |    0.01 | 0.0467 |     784 B |        1.00 |
| AspNetCore_AppendToUri       | AppendToUri |   348.1 ns |  3.77 ns |  3.53 ns |  0.93 |    0.01 | 0.0391 |     656 B |        0.84 |
| Flurl_AppendToUri            | AppendToUri | 3,327.3 ns | 18.28 ns | 17.10 ns |  8.88 |    0.08 | 0.3738 |    6312 B |        8.05 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Dictionary  | Dictionary  |   208.8 ns |  0.97 ns |  0.90 ns |  1.00 |    0.01 | 0.0324 |     544 B |        1.00 |
| AspNetCore_QueryHelpers      | Dictionary  |   224.3 ns |  0.87 ns |  0.81 ns |  1.07 |    0.01 | 0.0324 |     544 B |        1.00 |
| Flurl_Dictionary             | Dictionary  | 2,393.8 ns |  8.28 ns |  7.74 ns | 11.47 |    0.06 | 0.2823 |    4728 B |        8.69 |
| Bcl_Dictionary               | Dictionary  |   182.5 ns |  1.21 ns |  1.13 ns |  0.87 |    0.01 | 0.0291 |     488 B |        0.90 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Object      | Object      |   448.7 ns |  1.65 ns |  1.54 ns |  1.00 |    0.00 | 0.0482 |     808 B |        1.00 |
| Flurl_Object                 | Object      | 2,624.2 ns | 11.47 ns | 10.17 ns |  5.85 |    0.03 | 0.3090 |    5200 B |        6.44 |
