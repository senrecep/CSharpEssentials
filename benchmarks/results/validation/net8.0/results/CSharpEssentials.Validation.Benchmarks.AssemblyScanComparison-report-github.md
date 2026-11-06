```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials | 10.09 μs | 0.201 μs | 0.188 μs |  1.00 |    0.03 | 0.4883 |      - |   8.22 KB |        1.00 |
| FluentValidation | 24.44 μs | 0.489 μs | 0.543 μs |  2.42 |    0.07 | 1.9531 | 0.0305 |  31.94 KB |        3.88 |
