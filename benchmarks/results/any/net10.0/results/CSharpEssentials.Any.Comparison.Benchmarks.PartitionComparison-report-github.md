```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Partition  

```
| Method                     | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Partition | 6.281 μs | 0.1255 μs | 0.1395 μs |  1.00 |    0.03 | 1.1215 | 0.0229 |  18.33 KB |        1.00 |
| CSharpEssentials_HandLoop  | 3.309 μs | 0.0289 μs | 0.0270 μs |  0.53 |    0.01 | 1.1177 | 0.0114 |   18.3 KB |        1.00 |
| OneOf_HandLoop             | 3.387 μs | 0.0309 μs | 0.0289 μs |  0.54 |    0.01 | 1.1177 | 0.0114 |   18.3 KB |        1.00 |
| Dunet_StatefulMatch        | 9.107 μs | 0.0407 μs | 0.0381 μs |  1.45 |    0.03 | 1.1139 |      - |   18.3 KB |        1.00 |
