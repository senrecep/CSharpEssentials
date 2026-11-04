```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  cold   : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method   | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|----------:|------------:|
| MediatR  | 32.41 ms | 0.316 ms | 0.296 ms |  1.00 | 429.68 KB |        1.00 |
| Mediator | 35.55 ms | 0.353 ms | 0.330 ms |  1.10 | 364.01 KB |        0.85 |
