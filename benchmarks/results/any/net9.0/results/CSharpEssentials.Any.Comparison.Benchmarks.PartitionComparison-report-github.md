```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Partition  

```
| Method                     | Mean     | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Partition | 7.193 μs | 0.0182 μs | 0.0162 μs |  1.00 | 1.1292 | 0.0229 |  18.45 KB |        1.00 |
| CSharpEssentials_HandLoop  | 4.171 μs | 0.0377 μs | 0.0353 μs |  0.58 | 1.1215 | 0.0229 |  18.33 KB |        0.99 |
| OneOf_HandLoop             | 3.767 μs | 0.0388 μs | 0.0362 μs |  0.52 | 1.1215 | 0.0267 |  18.33 KB |        0.99 |
| Dunet_StatefulMatch        | 9.137 μs | 0.0569 μs | 0.0504 μs |  1.27 | 1.1139 | 0.0153 |  18.33 KB |        0.99 |
