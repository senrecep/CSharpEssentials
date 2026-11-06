```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method       | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Manual       |  24.73 ns | 0.118 ns | 0.110 ns |  1.00 |    0.01 | 0.0029 |      48 B |        1.00 |
| CseGenerated |  38.39 ns | 0.176 ns | 0.164 ns |  1.55 |    0.01 | 0.0029 |      48 B |        1.00 |
| CseRuntime   |  55.34 ns | 0.212 ns | 0.198 ns |  2.24 |    0.01 | 0.0029 |      48 B |        1.00 |
| Scrutor      | 163.95 ns | 0.225 ns | 0.188 ns |  6.63 |    0.03 | 0.0048 |      80 B |        1.67 |
