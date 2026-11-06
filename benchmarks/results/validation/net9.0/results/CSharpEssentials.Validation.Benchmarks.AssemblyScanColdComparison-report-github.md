```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method           | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| CSharpEssentials | 4.704 ms | 0.1244 ms | 0.1164 ms |  1.00 |    0.03 |  62.83 KB |        1.00 |
| FluentValidation | 5.399 ms | 0.2619 ms | 0.2449 ms |  1.15 |    0.06 |  67.01 KB |        1.07 |
