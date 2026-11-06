```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   721.2 ns | 10.09 ns |  9.44 ns | 0.1078 |      - |    1816 B |
| FV-Wide-Valid    |   807.3 ns |  4.01 ns |  3.55 ns | 0.0582 |      - |     984 B |
| CSE-Wide-Invalid | 1,393.4 ns | 10.40 ns |  9.72 ns | 0.2785 | 0.0019 |    4664 B |
| FV-Wide-Invalid  | 8,051.2 ns | 52.13 ns | 48.77 ns | 0.8240 | 0.0153 |   13952 B |
