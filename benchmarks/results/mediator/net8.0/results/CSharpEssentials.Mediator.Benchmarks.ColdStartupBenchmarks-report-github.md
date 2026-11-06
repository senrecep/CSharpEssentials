```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  cold   : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method   | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|----------:|------------:|
| MediatR  | 30.00 ms | 0.257 ms | 0.240 ms |  1.00 | 442.76 KB |        1.00 |
| Mediator | 32.98 ms | 0.292 ms | 0.273 ms |  1.10 | 363.36 KB |        0.82 |
