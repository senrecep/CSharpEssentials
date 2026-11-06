```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method             | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-CollectionItem |   669.2 ns | 1.88 ns | 1.67 ns | 0.0963 |   1.58 KB |
| FV-CollectionItem  | 1,638.7 ns | 5.01 ns | 4.69 ns | 0.1717 |   2.81 KB |
