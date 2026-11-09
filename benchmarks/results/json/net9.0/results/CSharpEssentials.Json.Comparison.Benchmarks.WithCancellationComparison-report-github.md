```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Completed | Completed  |  15.35 ns | 0.077 ns | 0.069 ns |  1.00 | 0.0043 |      72 B |        1.00 |
| Bcl_Completed              | Completed  |  16.37 ns | 0.093 ns | 0.087 ns |  1.07 | 0.0043 |      72 B |        1.00 |
|                            |            |           |          |          |       |        |           |             |
| CSharpEssentials_Pending   | Pending    | 172.73 ns | 0.746 ns | 0.698 ns |  1.00 | 0.0219 |     368 B |        1.00 |
| Bcl_Pending                | Pending    |  87.66 ns | 0.376 ns | 0.352 ns |  0.51 | 0.0148 |     248 B |        0.67 |
