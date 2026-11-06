```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error    | StdDev   | Gen0   | Allocated |
|----------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   306.2 ns |  0.54 ns |  0.45 ns | 0.0143 |     240 B |
| FV-MultiRegex-Valid    |   512.0 ns |  1.87 ns |  1.66 ns | 0.0372 |     632 B |
| CSE-MultiRegex-Invalid |   526.3 ns |  2.85 ns |  2.52 ns | 0.0610 |    1024 B |
| FV-MultiRegex-Invalid  | 2,589.5 ns | 27.04 ns | 23.97 ns | 0.2899 |    4872 B |
