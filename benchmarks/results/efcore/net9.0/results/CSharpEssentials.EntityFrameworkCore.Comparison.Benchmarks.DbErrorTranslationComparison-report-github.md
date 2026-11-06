```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                              | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| HandWritten_CatchSqliteException    | 122.3 μs | 0.71 μs | 0.63 μs |  1.00 | 1.2207 |  22.12 KB |        1.00 |
| CSharpEssentials_DbErrorTranslation | 124.2 μs | 1.68 μs | 1.57 μs |  1.02 | 1.2207 |  22.33 KB |        1.01 |
