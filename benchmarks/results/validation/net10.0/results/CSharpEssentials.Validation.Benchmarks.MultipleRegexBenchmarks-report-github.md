```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error    | StdDev   | Gen0   | Allocated |
|----------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   258.3 ns |  0.64 ns |  0.53 ns | 0.0105 |     176 B |
| FV-MultiRegex-Valid    |   465.1 ns |  2.39 ns |  2.24 ns | 0.0377 |     632 B |
| CSE-MultiRegex-Invalid |   386.7 ns |  3.07 ns |  2.87 ns | 0.0572 |     960 B |
| FV-MultiRegex-Invalid  | 2,311.0 ns | 26.71 ns | 24.98 ns | 0.2785 |    4680 B |
