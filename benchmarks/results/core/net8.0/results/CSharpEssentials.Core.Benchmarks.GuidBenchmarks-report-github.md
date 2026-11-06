```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| GuidNewGuid            | 750.46 ns | 14.534 ns | 14.274 ns |  1.00 |    0.03 |         - |          NA |
| GuiderNewGuid          |  72.40 ns |  0.053 ns |  0.047 ns |  0.10 |    0.00 |         - |          NA |
| GuiderNewMonotonicGuid |  86.79 ns |  0.301 ns |  0.267 ns |  0.12 |    0.00 |         - |          NA |
