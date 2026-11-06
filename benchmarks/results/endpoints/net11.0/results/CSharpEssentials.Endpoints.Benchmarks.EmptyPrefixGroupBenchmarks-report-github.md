```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method     | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|----------- |---------:|--------:|--------:|------:|----------:|------------:|
| Direct     | 801.4 ms | 2.35 ms | 2.08 ms |  1.00 |  15.05 MB |        1.00 |
| EmptyGroup | 804.7 ms | 3.62 ms | 3.39 ms |  1.00 |  15.54 MB |        1.03 |
