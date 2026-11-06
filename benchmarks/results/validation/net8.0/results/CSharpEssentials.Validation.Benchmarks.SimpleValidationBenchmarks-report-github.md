```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method             | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-Simple-Valid   |   421.3 ns |  1.48 ns |  1.38 ns | 0.0391 |     656 B |
| FV-Simple-Valid    |   459.3 ns |  2.21 ns |  2.06 ns | 0.0420 |     704 B |
| CSE-Simple-Invalid |   698.7 ns |  3.80 ns |  3.55 ns | 0.0925 |    1552 B |
| FV-Simple-Invalid  | 5,179.4 ns | 14.08 ns | 13.17 ns | 0.4501 |    7584 B |
