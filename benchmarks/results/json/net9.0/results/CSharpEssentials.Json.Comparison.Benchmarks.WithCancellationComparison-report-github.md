```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Completed | Completed  |  16.15 ns | 0.101 ns | 0.090 ns |  1.00 | 0.0043 |      72 B |        1.00 |
| Bcl_Completed              | Completed  |  16.48 ns | 0.100 ns | 0.088 ns |  1.02 | 0.0043 |      72 B |        1.00 |
|                            |            |           |          |          |       |        |           |             |
| CSharpEssentials_Pending   | Pending    | 213.02 ns | 0.518 ns | 0.460 ns |  1.00 | 0.0272 |     456 B |        1.00 |
| Bcl_Pending                | Pending    |  90.41 ns | 0.279 ns | 0.247 ns |  0.42 | 0.0148 |     248 B |        0.54 |
