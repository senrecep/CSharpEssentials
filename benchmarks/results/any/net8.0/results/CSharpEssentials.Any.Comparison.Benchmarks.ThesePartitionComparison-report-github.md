```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=Partition  

```
| Method                     | Mean     | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Partition | 9.977 μs | 0.0685 μs | 0.0641 μs |  1.00 | 1.7395 | 0.8545 |  28.58 KB |        1.00 |
| HandWritten_Partition      | 5.695 μs | 0.0496 μs | 0.0464 μs |  0.57 | 1.7471 | 0.8698 |  28.58 KB |        1.00 |
