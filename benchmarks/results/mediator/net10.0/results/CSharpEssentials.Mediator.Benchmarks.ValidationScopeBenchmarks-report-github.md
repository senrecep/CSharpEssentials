```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 554.6 ns | 4.56 ns | 4.04 ns |  1.00 | 0.0734 |    1232 B |        1.00 |
| Cse                 | 481.4 ns | 2.90 ns | 2.72 ns |  0.87 | 0.0582 |     976 B |        0.79 |
| MediatRHandWritten  | 530.9 ns | 1.84 ns | 1.63 ns |  0.96 | 0.0734 |    1232 B |        1.00 |
