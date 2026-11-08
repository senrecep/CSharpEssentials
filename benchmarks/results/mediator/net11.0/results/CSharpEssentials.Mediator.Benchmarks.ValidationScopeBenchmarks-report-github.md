```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatorHandWritten | 496.2 ns | 6.89 ns | 6.45 ns |  1.00 |    0.02 | 0.0725 |    1224 B |        1.00 |
| Cse                 | 448.2 ns | 3.05 ns | 2.70 ns |  0.90 |    0.01 | 0.0582 |     976 B |        0.80 |
| MediatRHandWritten  | 556.8 ns | 5.20 ns | 4.61 ns |  1.12 |    0.02 | 0.0725 |    1224 B |        1.00 |
