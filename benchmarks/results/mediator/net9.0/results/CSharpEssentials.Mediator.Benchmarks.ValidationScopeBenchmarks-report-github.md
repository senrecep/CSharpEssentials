```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 586.5 ns | 2.88 ns | 2.70 ns |  1.00 | 0.0734 |    1232 B |        1.00 |
| Cse                 | 499.0 ns | 2.88 ns | 2.70 ns |  0.85 | 0.0582 |     976 B |        0.79 |
| MediatRHandWritten  | 631.3 ns | 3.12 ns | 2.92 ns |  1.08 | 0.0734 |    1232 B |        1.00 |
