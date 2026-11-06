```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                 | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|----------------------- |---------:|---------:|---------:|------:|----------:|------------:|
| GuidNewGuid            | 57.94 ns | 0.021 ns | 0.017 ns |  1.00 |         - |          NA |
| GuidCreateVersion7     | 90.15 ns | 0.040 ns | 0.033 ns |  1.56 |         - |          NA |
| GuiderNewGuid          | 52.95 ns | 0.346 ns | 0.324 ns |  0.91 |         - |          NA |
| GuiderNewMonotonicGuid | 57.92 ns | 0.056 ns | 0.050 ns |  1.00 |         - |          NA |
