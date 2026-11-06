```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Partition  

```
| Method                     | Mean     | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Partition | 5.322 μs | 0.0528 μs | 0.0494 μs |  1.00 | 1.7471 | 0.8698 |  28.58 KB |        1.00 |
| HandWritten_Partition      | 5.077 μs | 0.0460 μs | 0.0407 μs |  0.95 | 1.7471 | 0.8698 |  28.55 KB |        1.00 |
