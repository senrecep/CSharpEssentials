```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   681.5 ns | 13.64 ns | 20.00 ns | 0.1020 |      - |    1720 B |
| FV-Wide-Valid    |   811.5 ns |  1.36 ns |  1.27 ns | 0.0582 |      - |     984 B |
| CSE-Wide-Invalid | 1,254.0 ns | 12.58 ns | 11.77 ns | 0.2728 | 0.0019 |    4568 B |
| FV-Wide-Invalid  | 7,429.1 ns | 29.89 ns | 27.96 ns | 0.8240 | 0.0153 |   13856 B |
