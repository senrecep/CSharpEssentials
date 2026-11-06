```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatorHandWritten | 198.6 ns | 0.15 ns | 0.14 ns |  1.00 |    0.00 | 0.0029 |      48 B |        1.00 |
| Cse                 | 186.6 ns | 0.54 ns | 0.51 ns |  0.94 |    0.00 | 0.0100 |     168 B |        3.50 |
| MediatRHandWritten  | 439.7 ns | 4.50 ns | 4.21 ns |  2.21 |    0.02 | 0.0348 |     584 B |       12.17 |
