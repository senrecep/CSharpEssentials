```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method        | Mean     | Error     | StdDev    | Median   | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|---------:|-------:|----------:|
| CSE-MustAsync | 1.263 μs | 0.0247 μs | 0.0573 μs | 1.241 μs | 0.0534 |     911 B |
| FV-MustAsync  | 1.641 μs | 0.0329 μs | 0.0707 μs | 1.621 μs | 0.0858 |    1463 B |
