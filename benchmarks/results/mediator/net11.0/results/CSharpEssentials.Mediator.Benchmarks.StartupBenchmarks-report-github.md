```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method   | Mean     | Error   | StdDev  | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| MediatR  | 216.3 μs | 1.63 μs | 1.45 μs |  1.00 | 21.4844 |      - | 369.94 KB |        1.00 |
| Mediator | 239.6 μs | 1.75 μs | 1.37 μs |  1.11 | 15.6250 | 3.9063 | 276.18 KB |        0.75 |
