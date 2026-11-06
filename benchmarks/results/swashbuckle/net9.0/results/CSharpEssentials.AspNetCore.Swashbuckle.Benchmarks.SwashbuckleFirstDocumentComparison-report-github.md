```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  cold   : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                          | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| Plain_AddSwaggerGen             | 650.7 ms |  8.32 ms |  7.79 ms |  1.00 |    0.02 |   7.91 MB |        1.00 |
| CSharpEssentials_AllConventions | 708.4 ms | 12.63 ms | 11.81 ms |  1.09 |    0.02 |   9.85 MB |        1.24 |
