```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatR  | 288.3 μs | 3.57 μs | 3.16 μs |  1.00 | 23.4375 |      - | 393.32 KB |        1.00 |
| Mediator | 225.6 μs | 0.66 μs | 0.62 μs |  0.78 | 16.8457 | 0.2441 | 277.18 KB |        0.70 |
