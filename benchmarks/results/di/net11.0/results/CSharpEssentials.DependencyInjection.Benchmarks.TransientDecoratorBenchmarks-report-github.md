```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method       | Mean      | Error    | StdDev   | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------:|---------:|---------:|----------:|------:|--------:|-------:|----------:|------------:|
| Manual       |  22.72 ns | 0.090 ns | 0.080 ns |  22.74 ns |  1.00 |    0.00 | 0.0029 |      48 B |        1.00 |
| CseGenerated |  38.98 ns | 0.137 ns | 0.122 ns |  38.96 ns |  1.72 |    0.01 | 0.0029 |      48 B |        1.00 |
| CseRuntime   |  57.80 ns | 1.170 ns | 1.922 ns |  56.66 ns |  2.54 |    0.08 | 0.0029 |      48 B |        1.00 |
| Scrutor      | 156.90 ns | 0.270 ns | 0.253 ns | 156.87 ns |  6.91 |    0.03 | 0.0048 |      80 B |        1.67 |
