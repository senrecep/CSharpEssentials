```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatR  | 202.6 μs | 2.31 μs | 2.16 μs |  1.00 | 21.4844 |      - | 369.94 KB |        1.00 |
| Mediator | 198.1 μs | 1.26 μs | 1.18 μs |  0.98 | 16.8457 | 0.2441 | 275.22 KB |        0.74 |
