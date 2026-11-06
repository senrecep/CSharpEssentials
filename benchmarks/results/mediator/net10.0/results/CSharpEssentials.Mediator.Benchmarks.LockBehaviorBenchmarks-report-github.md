```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 187.1 ns | 0.09 ns | 0.07 ns |  1.00 | 0.0029 |      48 B |        1.00 |
| Cse                 | 167.5 ns | 0.45 ns | 0.40 ns |  0.90 | 0.0100 |     168 B |        3.50 |
| MediatRHandWritten  | 339.9 ns | 1.15 ns | 1.08 ns |  1.82 | 0.0310 |     520 B |       10.83 |
