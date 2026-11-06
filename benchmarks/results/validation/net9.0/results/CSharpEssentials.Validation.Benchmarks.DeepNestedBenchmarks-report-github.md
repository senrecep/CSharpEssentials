```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method         | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-DeepNested |   652.8 ns | 2.52 ns | 2.35 ns | 0.0696 |   1.15 KB |
| FV-DeepNested  | 1,287.9 ns | 7.38 ns | 6.90 ns | 0.1240 |   2.04 KB |
