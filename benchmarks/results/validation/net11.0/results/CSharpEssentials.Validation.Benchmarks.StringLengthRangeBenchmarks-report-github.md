```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------------------ |-----------:|---------:|---------:|-------:|----------:|
| CSE-LengthRange-Valid   |   113.1 ns |  0.37 ns |  0.32 ns | 0.0181 |     304 B |
| FV-LengthRange-Valid    |   139.8 ns |  1.14 ns |  1.06 ns | 0.0358 |     600 B |
| CSE-LengthRange-Invalid |   252.9 ns |  1.78 ns |  1.66 ns | 0.0453 |     760 B |
| FV-LengthRange-Invalid  | 1,424.8 ns | 24.02 ns | 22.46 ns | 0.2270 |    3800 B |
