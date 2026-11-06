```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Categories  | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_AppendToUri | AppendToUri |   398.6 ns |  1.71 ns |  1.51 ns |  1.00 |    0.01 | 0.0467 |     784 B |        1.00 |
| AspNetCore_AppendToUri       | AppendToUri |   337.9 ns |  1.06 ns |  0.99 ns |  0.85 |    0.00 | 0.0391 |     656 B |        0.84 |
| Flurl_AppendToUri            | AppendToUri | 4,427.0 ns | 15.52 ns | 14.52 ns | 11.11 |    0.05 | 0.3891 |    6600 B |        8.42 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Dictionary  | Dictionary  |   223.2 ns |  0.48 ns |  0.45 ns |  1.00 |    0.00 | 0.0324 |     544 B |        1.00 |
| AspNetCore_QueryHelpers      | Dictionary  |   221.4 ns |  0.84 ns |  0.78 ns |  0.99 |    0.00 | 0.0324 |     544 B |        1.00 |
| Flurl_Dictionary             | Dictionary  | 3,084.1 ns | 13.60 ns | 12.05 ns | 13.82 |    0.06 | 0.2975 |    5016 B |        9.22 |
| Bcl_Dictionary               | Dictionary  |   184.8 ns |  0.94 ns |  0.88 ns |  0.83 |    0.00 | 0.0291 |     488 B |        0.90 |
|                              |             |            |          |          |       |         |        |           |             |
| CSharpEssentials_Object      | Object      |   478.6 ns |  2.97 ns |  2.63 ns |  1.00 |    0.01 | 0.0477 |     808 B |        1.00 |
| Flurl_Object                 | Object      | 3,258.2 ns | 11.03 ns |  9.78 ns |  6.81 |    0.04 | 0.3281 |    5488 B |        6.79 |
