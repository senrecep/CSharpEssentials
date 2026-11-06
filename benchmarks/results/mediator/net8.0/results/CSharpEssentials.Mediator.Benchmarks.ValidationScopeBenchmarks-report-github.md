```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 576.2 ns | 2.21 ns | 2.07 ns |  1.00 | 0.0715 |    1200 B |        1.00 |
| Cse                 | 514.4 ns | 2.88 ns | 2.69 ns |  0.89 | 0.0582 |     976 B |        0.81 |
| MediatRHandWritten  | 571.9 ns | 2.92 ns | 2.59 ns |  0.99 | 0.0715 |    1200 B |        1.00 |
