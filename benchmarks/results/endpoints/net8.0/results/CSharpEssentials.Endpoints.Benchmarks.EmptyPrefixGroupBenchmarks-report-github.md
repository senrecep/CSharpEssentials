```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method     | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|----------- |---------:|--------:|--------:|------:|----------:|------------:|
| Direct     | 541.1 ms | 4.19 ms | 3.92 ms |  1.00 |  15.01 MB |        1.00 |
| EmptyGroup | 546.9 ms | 2.16 ms | 1.92 ms |  1.01 |  15.55 MB |        1.04 |
