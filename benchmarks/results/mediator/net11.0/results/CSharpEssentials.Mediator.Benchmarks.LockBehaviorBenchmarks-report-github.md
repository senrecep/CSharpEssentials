```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|-------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| MediatorHandWritten | 203.3 ns | 1.73 ns | 1.54 ns |  1.00 | 0.0029 |      48 B |        1.00 |
| Cse                 | 189.5 ns | 0.73 ns | 0.69 ns |  0.93 | 0.0100 |     168 B |        3.50 |
| MediatRHandWritten  | 321.3 ns | 1.92 ns | 1.70 ns |  1.58 | 0.0300 |     504 B |       10.50 |
