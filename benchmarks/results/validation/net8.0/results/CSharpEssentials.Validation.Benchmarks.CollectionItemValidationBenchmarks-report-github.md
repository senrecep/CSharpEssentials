```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method             | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-CollectionItem |   715.1 ns | 2.84 ns | 2.65 ns | 0.0992 |   1.63 KB |
| FV-CollectionItem  | 1,902.4 ns | 4.49 ns | 3.98 ns | 0.1793 |   2.95 KB |
