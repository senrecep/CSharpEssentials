```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 205.3 ns | 0.31 ns | 0.28 ns |  1.00 | 0.0029 |      48 B |        1.00 |
| Cse                 | 177.5 ns | 0.28 ns | 0.24 ns |  0.86 | 0.0100 |     168 B |        3.50 |
| MediatRHandWritten  | 397.5 ns | 1.10 ns | 0.98 ns |  1.94 | 0.0348 |     584 B |       12.17 |
