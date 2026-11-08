```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method         | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| CseTypeName    | 8.071 μs | 0.0820 μs | 0.0767 μs |  1.00 |    0.01 |         - |          NA |
| CseCustom      | 7.609 μs | 0.1257 μs | 0.1176 μs |  0.94 |    0.02 |         - |          NA |
| DirectWithName | 6.108 μs | 0.0917 μs | 0.0813 μs |  0.76 |    0.01 |         - |          NA |
