```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| GuidNewGuid            | 752.17 ns | 13.557 ns | 12.682 ns |  1.00 |    0.02 |         - |          NA |
| GuidCreateVersion7     | 773.69 ns | 13.278 ns | 12.420 ns |  1.03 |    0.02 |         - |          NA |
| GuiderNewGuid          |  59.39 ns |  0.034 ns |  0.027 ns |  0.08 |    0.00 |         - |          NA |
| GuiderNewMonotonicGuid |  71.34 ns |  0.360 ns |  0.337 ns |  0.09 |    0.00 |         - |          NA |
