```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method   | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|----------:|------------:|
| MediatR  | 29.33 ms | 0.331 ms | 0.309 ms |  1.00 | 428.38 KB |        1.00 |
| Mediator | 31.42 ms | 0.146 ms | 0.137 ms |  1.07 | 363.34 KB |        0.85 |
