```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method   | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| MediatR  | 33.31 ms | 0.571 ms | 0.534 ms |  1.00 |    0.02 | 420.87 KB |        1.00 |
| Mediator | 36.41 ms | 0.195 ms | 0.182 ms |  1.09 |    0.02 | 387.27 KB |        0.92 |
