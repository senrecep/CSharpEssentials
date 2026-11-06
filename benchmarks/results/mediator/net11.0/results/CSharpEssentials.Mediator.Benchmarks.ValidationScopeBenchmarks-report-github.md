```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 512.3 ns | 2.53 ns | 2.37 ns |  1.00 | 0.0725 |    1224 B |        1.00 |
| Cse                 | 464.7 ns | 3.22 ns | 3.02 ns |  0.91 | 0.0582 |     976 B |        0.80 |
| MediatRHandWritten  | 509.4 ns | 4.21 ns | 3.93 ns |  0.99 | 0.0725 |    1224 B |        1.00 |
