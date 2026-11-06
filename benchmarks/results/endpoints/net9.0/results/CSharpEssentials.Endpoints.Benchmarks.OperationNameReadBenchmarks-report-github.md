```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method         | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| CseTypeName    | 11.118 μs | 0.1658 μs | 0.1551 μs |  1.00 |    0.02 |         - |          NA |
| CseCustom      | 11.348 μs | 0.1292 μs | 0.1209 μs |  1.02 |    0.02 |         - |          NA |
| DirectWithName |  8.983 μs | 0.1161 μs | 0.1086 μs |  0.81 |    0.01 |         - |          NA |
