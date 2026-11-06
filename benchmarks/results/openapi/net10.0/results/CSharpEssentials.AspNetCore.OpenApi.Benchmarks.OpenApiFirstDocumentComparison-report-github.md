```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  cold   : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                          | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |---------:|--------:|--------:|------:|----------:|------------:|
| Plain_AddOpenApi                | 762.4 ms | 4.70 ms | 4.39 ms |  1.00 |   8.72 MB |        1.00 |
| CSharpEssentials_AllConventions | 828.4 ms | 4.80 ms | 4.49 ms |  1.09 |  10.89 MB |        1.25 |
