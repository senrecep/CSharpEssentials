```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  cold   : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| MediatR  | 33.06 ms | 0.366 ms | 0.342 ms |  1.00 |    0.01 |  429.4 KB |        1.00 |
| Mediator | 36.56 ms | 0.510 ms | 0.477 ms |  1.11 |    0.02 | 388.76 KB |        0.91 |
