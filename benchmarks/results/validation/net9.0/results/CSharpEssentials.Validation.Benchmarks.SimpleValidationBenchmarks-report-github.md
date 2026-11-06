```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method             | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-Simple-Valid   |   389.5 ns |  1.65 ns |  1.47 ns | 0.0391 |     656 B |
| FV-Simple-Valid    |   475.9 ns |  1.77 ns |  1.66 ns | 0.0420 |     704 B |
| CSE-Simple-Invalid |   627.2 ns |  3.21 ns |  3.00 ns | 0.0906 |    1528 B |
| FV-Simple-Invalid  | 4,361.0 ns | 14.31 ns | 12.68 ns | 0.4501 |    7584 B |
