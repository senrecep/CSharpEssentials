```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method         | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| CseTypeName    | 8.505 μs | 0.1487 μs | 0.1391 μs |  1.00 |    0.02 |         - |          NA |
| CseCustom      | 8.404 μs | 0.1057 μs | 0.0989 μs |  0.99 |    0.02 |         - |          NA |
| DirectWithName | 6.960 μs | 0.0866 μs | 0.0810 μs |  0.82 |    0.02 |         - |          NA |
