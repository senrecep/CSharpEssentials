```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_Completed | Completed  |  12.79 ns | 0.111 ns | 0.104 ns |  1.00 | 0.0043 |      72 B |        1.00 |
| Bcl_Completed              | Completed  |  12.98 ns | 0.069 ns | 0.057 ns |  1.01 | 0.0043 |      72 B |        1.00 |
|                            |            |           |          |          |       |        |           |             |
| CSharpEssentials_Pending   | Pending    | 173.61 ns | 1.099 ns | 1.028 ns |  1.00 | 0.0219 |     368 B |        1.00 |
| Bcl_Pending                | Pending    |  84.40 ns | 0.496 ns | 0.464 ns |  0.49 | 0.0148 |     248 B |        0.67 |
