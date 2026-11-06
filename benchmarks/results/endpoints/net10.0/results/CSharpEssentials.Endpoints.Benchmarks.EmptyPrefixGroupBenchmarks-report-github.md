```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method     | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|----------- |---------:|--------:|--------:|------:|----------:|------------:|
| Direct     | 757.0 ms | 2.65 ms | 2.35 ms |  1.00 |   15.1 MB |        1.00 |
| EmptyGroup | 759.5 ms | 2.17 ms | 2.03 ms |  1.00 |   15.6 MB |        1.03 |
