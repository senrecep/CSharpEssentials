```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  cold   : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Job=cold  IterationCount=1  LaunchCount=15  
RunStrategy=ColdStart  WarmupCount=0  

```
| Method                          | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| Plain_AddSwaggerGen             | 702.3 ms | 11.54 ms | 10.80 ms |  1.00 |    0.02 |    8.2 MB |        1.00 |
| CSharpEssentials_AllConventions | 761.8 ms |  8.80 ms |  8.24 ms |  1.09 |    0.02 |  10.17 MB |        1.24 |
