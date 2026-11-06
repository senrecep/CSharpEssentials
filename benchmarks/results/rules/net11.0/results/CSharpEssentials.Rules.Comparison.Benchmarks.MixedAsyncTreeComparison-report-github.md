```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         | 3.377 μs | 0.0334 μs | 0.0313 μs |  1.00 |    0.01 | 0.0229 |     485 B |        1.00 |
| CseEvaluateAsync         | 3.578 μs | 0.0345 μs | 0.0306 μs |  1.06 |    0.01 | 0.0534 |     982 B |        2.02 |
| CseEvaluateSyncOverAsync | 3.520 μs | 0.0455 μs | 0.0426 μs |  1.04 |    0.02 | 0.0305 |     592 B |        1.22 |
