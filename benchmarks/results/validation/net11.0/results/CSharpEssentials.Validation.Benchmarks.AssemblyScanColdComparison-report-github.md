```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method           | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| CSharpEssentials | 4.781 ms | 0.2066 ms | 0.1933 ms |  1.00 |    0.05 |  67.32 KB |        1.00 |
| FluentValidation | 4.957 ms | 0.1047 ms | 0.0980 ms |  1.04 |    0.04 |  66.87 KB |        0.99 |
