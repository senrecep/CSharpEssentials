```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|----------:|------------:|
| GuidNewGuid            | 759.69 ns | 6.266 ns | 5.862 ns |  1.00 |         - |          NA |
| GuidCreateVersion7     | 805.80 ns | 7.290 ns | 6.819 ns |  1.06 |         - |          NA |
| GuiderNewGuid          |  71.90 ns | 0.019 ns | 0.017 ns |  0.09 |         - |          NA |
| GuiderNewMonotonicGuid |  83.61 ns | 0.168 ns | 0.157 ns |  0.11 |         - |          NA |
