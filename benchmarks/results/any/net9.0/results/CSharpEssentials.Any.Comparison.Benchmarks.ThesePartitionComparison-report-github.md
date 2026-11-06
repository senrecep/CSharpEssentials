```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Partition  

```
| Method                     | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Partition | 8.086 μs | 0.1447 μs | 0.1354 μs |  1.00 |    0.02 | 1.7395 | 0.8545 |  28.58 KB |        1.00 |
| HandWritten_Partition      | 5.008 μs | 0.0380 μs | 0.0337 μs |  0.62 |    0.01 | 1.7471 | 0.8698 |  28.58 KB |        1.00 |
