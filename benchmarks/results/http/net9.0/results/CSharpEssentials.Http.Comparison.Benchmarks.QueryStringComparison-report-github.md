```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_AppendToUri | AppendToUri |   380.6 ns |  1.00 ns |  0.94 ns |  1.00 |    0.00 | 0.0467 |     784 B |        1.00 |
| AspNetCore_AppendToUri       | AppendToUri |   331.2 ns |  1.78 ns |  1.67 ns |  0.87 |    0.00 | 0.0391 |     656 B |        0.84 |
| Flurl_AppendToUri            | AppendToUri | 4,323.0 ns |  9.23 ns |  7.71 ns | 11.36 |    0.03 | 0.3891 |    6600 B |        8.42 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Dictionary  | Dictionary  |   216.2 ns |  0.79 ns |  0.74 ns |  1.00 |    0.00 | 0.0324 |     544 B |        1.00 |
| AspNetCore_QueryHelpers      | Dictionary  |   194.2 ns |  1.60 ns |  1.50 ns |  0.90 |    0.01 | 0.0324 |     544 B |        1.00 |
| Flurl_Dictionary             | Dictionary  | 3,195.2 ns | 15.53 ns | 14.53 ns | 14.78 |    0.08 | 0.2975 |    5016 B |        9.22 |
| Bcl_Dictionary               | Dictionary  |   194.4 ns |  0.88 ns |  0.78 ns |  0.90 |    0.00 | 0.0291 |     488 B |        0.90 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Object      | Object      |   460.7 ns |  1.72 ns |  1.53 ns |  1.00 |    0.00 | 0.0482 |     808 B |        1.00 |
| Flurl_Object                 | Object      | 3,165.2 ns | 16.28 ns | 15.23 ns |  6.87 |    0.04 | 0.3281 |    5488 B |        6.79 |
