```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=Partition  

```
| Method                     | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Partition | 5.000 μs | 0.0987 μs | 0.1283 μs |  1.00 |    0.04 | 1.7471 | 0.8698 |  28.58 KB |        1.00 |
| HandWritten_Partition      | 4.969 μs | 0.0680 μs | 0.0603 μs |  0.99 |    0.03 | 1.7471 | 0.8698 |  28.55 KB |        1.00 |
