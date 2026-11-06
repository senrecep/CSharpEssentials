```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                              | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| HandWritten_CatchSqliteException    | 71.29 μs | 0.217 μs | 0.193 μs |  1.00 | 1.2207 |  21.81 KB |        1.00 |
| EfExceptions_UseExceptionProcessor  | 77.41 μs | 0.307 μs | 0.272 μs |  1.09 | 1.2207 |  21.51 KB |        0.99 |
| CSharpEssentials_DbErrorTranslation | 70.90 μs | 0.207 μs | 0.184 μs |  0.99 | 1.2207 |  22.02 KB |        1.01 |
