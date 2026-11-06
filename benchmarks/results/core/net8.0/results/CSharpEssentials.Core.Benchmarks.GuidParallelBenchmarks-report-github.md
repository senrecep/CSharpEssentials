```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|----------:|------------:|
| GuidNewGuid            | 150.43 ns | 1.652 ns | 1.379 ns |  1.00 |         - |          NA |
| GuiderNewGuid          |  16.15 ns | 0.272 ns | 0.279 ns |  0.11 |         - |          NA |
| GuiderNewMonotonicGuid |  56.79 ns | 0.246 ns | 0.230 ns |  0.38 |         - |          NA |
