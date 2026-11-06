```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Completed | Completed  |  16.64 ns | 0.081 ns | 0.075 ns |  1.00 | 0.0043 |      72 B |        1.00 |
| Bcl_Completed              | Completed  |  17.48 ns | 0.141 ns | 0.132 ns |  1.05 | 0.0043 |      72 B |        1.00 |
|                            |            |           |          |          |       |        |           |             |
| CSharpEssentials_Pending   | Pending    | 283.00 ns | 2.924 ns | 2.735 ns |  1.00 | 0.0348 |     584 B |        1.00 |
| Bcl_Pending                | Pending    | 105.36 ns | 0.858 ns | 0.802 ns |  0.37 | 0.0162 |     272 B |        0.47 |
