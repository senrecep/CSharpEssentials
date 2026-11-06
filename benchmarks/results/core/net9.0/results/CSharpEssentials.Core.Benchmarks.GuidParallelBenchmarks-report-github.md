```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| GuidNewGuid            | 150.82 ns | 1.422 ns | 1.187 ns |  1.00 |    0.01 |         - |          NA |
| GuidCreateVersion7     | 160.20 ns | 2.793 ns | 2.332 ns |  1.06 |    0.02 |         - |          NA |
| GuiderNewGuid          |  15.67 ns | 0.236 ns | 0.210 ns |  0.10 |    0.00 |         - |          NA |
| GuiderNewMonotonicGuid |  45.86 ns | 0.587 ns | 0.520 ns |  0.30 |    0.00 |         - |          NA |
