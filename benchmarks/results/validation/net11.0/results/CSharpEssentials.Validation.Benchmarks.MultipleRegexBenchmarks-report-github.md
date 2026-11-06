```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error    | StdDev   | Gen0   | Allocated |
|----------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   249.5 ns |  0.66 ns |  0.62 ns | 0.0105 |     176 B |
| FV-MultiRegex-Valid    |   457.7 ns |  0.88 ns |  0.74 ns | 0.0377 |     632 B |
| CSE-MultiRegex-Invalid |   380.1 ns |  1.32 ns |  1.24 ns | 0.0572 |     960 B |
| FV-MultiRegex-Invalid  | 2,188.6 ns | 13.11 ns | 12.26 ns | 0.2747 |    4656 B |
