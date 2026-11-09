```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| HandWrittenAsync         |   771.9 ns | 5.62 ns | 5.26 ns |  1.00 | 0.0286 |     485 B |        1.00 |
| CseEvaluateAsync         | 1,217.2 ns | 7.36 ns | 6.53 ns |  1.58 | 0.0381 |     669 B |        1.38 |
| CseEvaluateSyncOverAsync |   828.1 ns | 4.37 ns | 4.09 ns |  1.07 | 0.0353 |     591 B |        1.22 |
