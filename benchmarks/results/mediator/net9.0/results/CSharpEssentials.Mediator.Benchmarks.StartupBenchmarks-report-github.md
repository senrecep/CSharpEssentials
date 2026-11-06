```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------- |---------:|--------:|--------:|------:|--------:|--------:|-------:|----------:|------------:|
| MediatR  | 274.1 μs | 3.64 μs | 3.41 μs |  1.00 |    0.02 | 21.4844 |      - | 378.54 KB |        1.00 |
| Mediator | 221.4 μs | 1.64 μs | 1.37 μs |  0.81 |    0.01 | 17.8223 | 8.5449 | 291.66 KB |        0.77 |
