```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   732.0 ns |  6.17 ns |  5.77 ns | 0.1078 |      - |    1816 B |
| FV-Wide-Valid    |   951.5 ns |  4.14 ns |  3.87 ns | 0.0582 |      - |     984 B |
| CSE-Wide-Invalid | 1,496.0 ns |  5.23 ns |  4.64 ns | 0.2785 | 0.0019 |    4664 B |
| FV-Wide-Invalid  | 8,887.9 ns | 40.47 ns | 37.86 ns | 0.8698 | 0.0153 |   14720 B |
