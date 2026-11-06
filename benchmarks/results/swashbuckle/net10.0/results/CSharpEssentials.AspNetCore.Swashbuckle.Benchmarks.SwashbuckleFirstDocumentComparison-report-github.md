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
| Plain_AddSwaggerGen             | 703.9 ms | 5.91 ms | 5.53 ms |  1.00 |   8.18 MB |        1.00 |
| CSharpEssentials_AllConventions | 763.0 ms | 6.36 ms | 5.95 ms |  1.08 |  10.03 MB |        1.23 |
