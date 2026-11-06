```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                              | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWritten_CatchSqliteException    | 88.17 μs | 1.039 μs | 0.921 μs |  1.00 |    0.01 | 1.2207 |  21.84 KB |        1.00 |
| EfExceptions_UseExceptionProcessor  | 92.39 μs | 1.614 μs | 1.510 μs |  1.05 |    0.02 | 1.2207 |  21.49 KB |        0.98 |
| CSharpEssentials_DbErrorTranslation | 85.75 μs | 1.102 μs | 1.031 μs |  0.97 |    0.02 | 1.3428 |  22.05 KB |        1.01 |
