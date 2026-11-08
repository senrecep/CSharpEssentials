```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 554.7 ns | 3.21 ns | 3.01 ns |  1.00 | 0.0715 |    1200 B |        1.00 |
| Cse                 | 506.7 ns | 3.06 ns | 2.86 ns |  0.91 | 0.0582 |     976 B |        0.81 |
| MediatRHandWritten  | 561.5 ns | 3.07 ns | 2.87 ns |  1.01 | 0.0715 |    1200 B |        1.00 |
