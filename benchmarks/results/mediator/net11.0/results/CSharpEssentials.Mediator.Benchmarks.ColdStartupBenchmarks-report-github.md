```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method   | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|----------:|------------:|
| MediatR  | 32.65 ms | 0.324 ms | 0.303 ms |  1.00 | 420.87 KB |        1.00 |
| Mediator | 35.86 ms | 0.199 ms | 0.186 ms |  1.10 | 362.94 KB |        0.86 |
