```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Cascade-Stop |   195.7 ns | 1.28 ns | 1.19 ns | 0.0381 |     640 B |
| FV-Cascade-Stop  | 1,261.9 ns | 6.12 ns | 5.73 ns | 0.2003 |    3352 B |
