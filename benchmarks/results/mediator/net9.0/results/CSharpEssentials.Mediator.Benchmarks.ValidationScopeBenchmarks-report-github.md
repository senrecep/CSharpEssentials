```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method              | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|---------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatorHandWritten | 559.5 ns |  3.30 ns | 2.93 ns |  1.00 |    0.01 | 0.0734 |    1232 B |        1.00 |
| Cse                 | 492.4 ns |  2.83 ns | 2.64 ns |  0.88 |    0.01 | 0.0582 |     976 B |        0.79 |
| MediatRHandWritten  | 558.4 ns | 10.08 ns | 9.43 ns |  1.00 |    0.02 | 0.0734 |    1232 B |        1.00 |
