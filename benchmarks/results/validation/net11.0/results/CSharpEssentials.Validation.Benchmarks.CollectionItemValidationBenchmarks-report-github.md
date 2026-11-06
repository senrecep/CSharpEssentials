```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method             | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-CollectionItem |   484.4 ns | 3.52 ns | 3.12 ns | 0.0725 |    1.2 KB |
| FV-CollectionItem  | 1,432.9 ns | 3.75 ns | 3.33 ns | 0.1774 |   2.92 KB |
