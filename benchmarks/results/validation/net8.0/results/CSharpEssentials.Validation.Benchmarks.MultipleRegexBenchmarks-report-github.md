```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   301.8 ns | 0.58 ns | 0.54 ns | 0.0143 |     240 B |
| FV-MultiRegex-Valid    |   561.1 ns | 1.51 ns | 1.41 ns | 0.0372 |     632 B |
| CSE-MultiRegex-Invalid |   495.0 ns | 1.51 ns | 1.41 ns | 0.0610 |    1024 B |
| FV-MultiRegex-Invalid  | 2,794.3 ns | 7.83 ns | 7.32 ns | 0.2899 |    4872 B |
