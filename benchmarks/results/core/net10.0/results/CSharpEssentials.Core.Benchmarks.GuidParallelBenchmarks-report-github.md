```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| GuidNewGuid            | 151.70 ns | 2.422 ns | 2.265 ns |  1.00 |    0.02 |         - |          NA |
| GuidCreateVersion7     | 158.37 ns | 3.091 ns | 2.891 ns |  1.04 |    0.02 |         - |          NA |
| GuiderNewGuid          |  13.54 ns | 0.268 ns | 0.275 ns |  0.09 |    0.00 |         - |          NA |
| GuiderNewMonotonicGuid |  45.28 ns | 0.632 ns | 0.591 ns |  0.30 |    0.01 |         - |          NA |
