```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method         | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| CseTypeName    | 11.536 μs | 0.1120 μs | 0.1048 μs |  1.00 |    0.01 |         - |          NA |
| CseCustom      | 11.350 μs | 0.2081 μs | 0.1946 μs |  0.98 |    0.02 |         - |          NA |
| DirectWithName |  9.545 μs | 0.0539 μs | 0.0505 μs |  0.83 |    0.01 |         - |          NA |
