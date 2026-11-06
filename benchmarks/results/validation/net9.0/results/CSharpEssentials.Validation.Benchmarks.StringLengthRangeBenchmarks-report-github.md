```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                  | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------------------ |-----------:|---------:|---------:|-------:|----------:|
| CSE-LengthRange-Valid   |   149.8 ns |  0.89 ns |  0.83 ns | 0.0224 |     376 B |
| FV-LengthRange-Valid    |   180.6 ns |  1.68 ns |  1.57 ns | 0.0358 |     600 B |
| CSE-LengthRange-Invalid |   295.3 ns |  1.47 ns |  1.30 ns | 0.0496 |     832 B |
| FV-LengthRange-Invalid  | 1,714.1 ns | 21.75 ns | 20.35 ns | 0.2308 |    3872 B |
