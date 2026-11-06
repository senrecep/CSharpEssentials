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
| CSharpEssentials_Partition | 8.803 μs | 0.0644 μs | 0.0571 μs |  1.00 | 1.1292 | 0.0153 |  18.45 KB |        1.00 |
| CSharpEssentials_HandLoop  | 4.380 μs | 0.0418 μs | 0.0349 μs |  0.50 | 1.1215 | 0.0229 |  18.33 KB |        0.99 |
| OneOf_HandLoop             | 5.338 μs | 0.0748 μs | 0.0700 μs |  0.61 | 1.1215 | 0.0229 |  18.33 KB |        0.99 |
| Dunet_StatefulMatch        | 9.357 μs | 0.0847 μs | 0.0792 μs |  1.06 | 1.1139 | 0.0153 |  18.33 KB |        0.99 |
