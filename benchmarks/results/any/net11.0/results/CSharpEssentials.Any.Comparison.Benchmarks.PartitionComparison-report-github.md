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
| CSharpEssentials_Partition | 5.148 μs | 0.0719 μs | 0.0637 μs |  1.00 |    0.02 | 1.1215 | 0.0229 |  18.33 KB |        1.00 |
| CSharpEssentials_HandLoop  | 3.287 μs | 0.0304 μs | 0.0285 μs |  0.64 |    0.01 | 1.1177 | 0.0114 |   18.3 KB |        1.00 |
| OneOf_HandLoop             | 3.307 μs | 0.0310 μs | 0.0290 μs |  0.64 |    0.01 | 1.1177 | 0.0114 |   18.3 KB |        1.00 |
| Dunet_StatefulMatch        | 8.982 μs | 0.1197 μs | 0.1000 μs |  1.75 |    0.03 | 1.1139 |      - |   18.3 KB |        1.00 |
