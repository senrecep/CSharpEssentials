```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 511.8 ns | 2.73 ns | 2.55 ns |  1.00 | 0.0734 |    1232 B |        1.00 |
| Cse                 | 469.3 ns | 2.51 ns | 2.35 ns |  0.92 | 0.0582 |     976 B |        0.79 |
| MediatRHandWritten  | 519.8 ns | 3.69 ns | 3.45 ns |  1.02 | 0.0734 |    1232 B |        1.00 |
