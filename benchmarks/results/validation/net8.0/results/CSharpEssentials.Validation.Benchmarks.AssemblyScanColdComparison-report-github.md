```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  cold   : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method           | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| CSharpEssentials | 5.038 ms | 0.0651 ms | 0.0609 ms |  1.00 |    0.02 |  62.83 KB |        1.00 |
| FluentValidation | 5.728 ms | 0.1254 ms | 0.1173 ms |  1.14 |    0.03 |  67.01 KB |        1.07 |
