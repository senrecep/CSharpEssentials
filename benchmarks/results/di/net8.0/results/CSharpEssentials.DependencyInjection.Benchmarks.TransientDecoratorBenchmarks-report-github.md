```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method       | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Manual       |  30.68 ns | 0.121 ns | 0.108 ns |  1.00 |    0.00 | 0.0029 |      48 B |        1.00 |
| CseGenerated |  45.29 ns | 0.082 ns | 0.073 ns |  1.48 |    0.01 | 0.0029 |      48 B |        1.00 |
| CseRuntime   |  61.24 ns | 0.155 ns | 0.138 ns |  2.00 |    0.01 | 0.0029 |      48 B |        1.00 |
| Scrutor      | 165.75 ns | 0.269 ns | 0.252 ns |  5.40 |    0.02 | 0.0048 |      80 B |        1.67 |
