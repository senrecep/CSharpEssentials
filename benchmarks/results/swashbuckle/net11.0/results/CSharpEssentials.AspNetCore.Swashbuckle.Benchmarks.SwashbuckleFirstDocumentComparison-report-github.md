```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  cold   : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                          | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |---------:|--------:|--------:|------:|----------:|------------:|
| Plain_AddSwaggerGen             | 726.9 ms | 5.35 ms | 5.00 ms |  1.00 |   8.15 MB |        1.00 |
| CSharpEssentials_AllConventions | 789.2 ms | 6.13 ms | 5.73 ms |  1.09 |   9.93 MB |        1.22 |
