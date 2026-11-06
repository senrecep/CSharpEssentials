```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method     | Mean     | Error   | StdDev  | Ratio | Allocated | Alloc Ratio |
|----------- |---------:|--------:|--------:|------:|----------:|------------:|
| Direct     | 687.5 ms | 1.50 ms | 1.40 ms |  1.00 |  15.12 MB |        1.00 |
| EmptyGroup | 688.0 ms | 1.57 ms | 1.22 ms |  1.00 |  15.68 MB |        1.04 |
