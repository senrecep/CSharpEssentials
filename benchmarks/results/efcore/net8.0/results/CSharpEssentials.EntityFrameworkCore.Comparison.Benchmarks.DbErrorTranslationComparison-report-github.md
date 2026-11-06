```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| HandWritten_CatchSqliteException    | 234.9 μs | 0.48 μs | 0.43 μs |  1.00 | 1.4648 |  26.16 KB |        1.00 |
| CSharpEssentials_DbErrorTranslation | 233.7 μs | 0.47 μs | 0.42 μs |  1.00 | 1.4648 |  27.89 KB |        1.07 |
