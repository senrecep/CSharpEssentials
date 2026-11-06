```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                 | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| GuidNewGuid            | 12.58 ns | 0.148 ns | 0.138 ns |  1.00 |    0.02 |         - |          NA |
| GuidCreateVersion7     | 18.15 ns | 0.164 ns | 0.145 ns |  1.44 |    0.02 |         - |          NA |
| GuiderNewGuid          | 12.61 ns | 0.146 ns | 0.129 ns |  1.00 |    0.01 |         - |          NA |
| GuiderNewMonotonicGuid | 50.73 ns | 0.835 ns | 0.781 ns |  4.03 |    0.07 |         - |          NA |
