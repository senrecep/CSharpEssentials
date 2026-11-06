```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   774.2 ns |  7.78 ns |  6.90 ns | 0.1078 |      - |    1816 B |
| FV-Wide-Valid    | 1,132.0 ns |  2.12 ns |  1.98 ns | 0.0572 |      - |     984 B |
| CSE-Wide-Invalid | 1,604.7 ns |  8.86 ns |  6.92 ns | 0.2785 | 0.0019 |    4664 B |
| FV-Wide-Invalid  | 9,267.2 ns | 36.19 ns | 33.85 ns | 0.8698 | 0.0153 |   14720 B |
