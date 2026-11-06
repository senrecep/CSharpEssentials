```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method             | Mean       | Error    | StdDev  | Gen0   | Allocated |
|------------------- |-----------:|---------:|--------:|-------:|----------:|
| CSE-CollectionItem |   508.4 ns |  3.80 ns | 3.37 ns | 0.0734 |    1.2 KB |
| FV-CollectionItem  | 1,430.0 ns | 10.16 ns | 9.50 ns | 0.1774 |   2.92 KB |
