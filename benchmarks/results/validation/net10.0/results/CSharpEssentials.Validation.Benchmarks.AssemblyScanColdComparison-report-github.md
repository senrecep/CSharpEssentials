```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  cold   : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method           | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| CSharpEssentials | 5.346 ms | 0.0525 ms | 0.0491 ms |  1.00 |    0.01 |  67.38 KB |        1.00 |
| FluentValidation | 5.695 ms | 0.1151 ms | 0.1077 ms |  1.07 |    0.02 |  68.13 KB |        1.01 |
