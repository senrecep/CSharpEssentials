```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method       | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Manual       |  27.17 ns | 0.162 ns | 0.152 ns |  1.00 |    0.01 | 0.0029 |      48 B |        1.00 |
| CseGenerated |  39.27 ns | 0.304 ns | 0.284 ns |  1.45 |    0.01 | 0.0029 |      48 B |        1.00 |
| CseRuntime   |  50.22 ns | 0.350 ns | 0.310 ns |  1.85 |    0.01 | 0.0029 |      48 B |        1.00 |
| Scrutor      | 160.36 ns | 0.783 ns | 0.732 ns |  5.90 |    0.04 | 0.0048 |      80 B |        1.67 |
