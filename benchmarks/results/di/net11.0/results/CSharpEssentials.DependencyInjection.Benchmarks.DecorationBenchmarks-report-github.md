```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method       | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |   879.0 ns |  6.57 ns |  6.15 ns |  1.00 |    0.01 | 0.2995 | 0.0048 |    4.9 KB |        1.00 |
| CseGenerated | 3,459.0 ns | 18.99 ns | 17.76 ns |  3.94 |    0.03 | 0.6485 | 0.0153 |   10.6 KB |        2.16 |
| CseRuntime   | 4,581.9 ns | 11.85 ns |  9.90 ns |  5.21 |    0.04 | 0.7935 | 0.0229 |  13.01 KB |        2.66 |
| Scrutor      | 3,980.5 ns | 35.76 ns | 33.45 ns |  4.53 |    0.05 | 0.6866 | 0.0153 |  11.31 KB |        2.31 |
