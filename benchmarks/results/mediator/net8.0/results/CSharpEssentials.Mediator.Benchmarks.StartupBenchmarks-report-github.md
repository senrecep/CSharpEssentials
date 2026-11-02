```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatR  | 286.3 μs | 2.10 μs | 1.86 μs |  1.00 | 23.4375 |      - | 393.32 KB |        1.00 |
| Mediator | 219.1 μs | 0.93 μs | 0.87 μs |  0.77 | 16.8457 | 0.2441 | 277.18 KB |        0.70 |
