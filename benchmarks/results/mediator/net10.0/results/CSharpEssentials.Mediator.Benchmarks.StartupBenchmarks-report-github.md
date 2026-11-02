```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatR  | 253.4 μs | 2.55 μs | 2.26 μs |  1.00 | 21.4844 |      - | 378.42 KB |        1.00 |
| Mediator | 228.0 μs | 3.20 μs | 2.99 μs |  0.90 | 17.5781 | 5.8594 | 292.36 KB |        0.77 |
