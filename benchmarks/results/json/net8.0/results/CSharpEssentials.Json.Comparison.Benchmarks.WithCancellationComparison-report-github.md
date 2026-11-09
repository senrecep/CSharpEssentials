```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Completed | Completed  |  16.07 ns | 0.189 ns | 0.168 ns |  1.00 | 0.0043 |      72 B |        1.00 |
| Bcl_Completed              | Completed  |  16.73 ns | 0.139 ns | 0.130 ns |  1.04 | 0.0043 |      72 B |        1.00 |
|                            |            |           |          |          |       |        |           |             |
| CSharpEssentials_Pending   | Pending    | 291.25 ns | 1.400 ns | 1.309 ns |  1.00 | 0.0348 |     584 B |        1.00 |
| Bcl_Pending                | Pending    | 102.30 ns | 0.641 ns | 0.599 ns |  0.35 | 0.0162 |     272 B |        0.47 |
