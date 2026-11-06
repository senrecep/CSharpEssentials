```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method             | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Simple-Valid   |   317.7 ns |  1.09 ns |  1.02 ns | 0.0339 |      - |     568 B |
| FV-Simple-Valid    |   391.9 ns |  5.17 ns |  4.58 ns | 0.0434 |      - |     728 B |
| CSE-Simple-Invalid |   493.7 ns |  3.04 ns |  2.70 ns | 0.0858 |      - |    1440 B |
| FV-Simple-Invalid  | 3,585.1 ns | 15.32 ns | 14.33 ns | 0.4311 | 0.0038 |    7248 B |
