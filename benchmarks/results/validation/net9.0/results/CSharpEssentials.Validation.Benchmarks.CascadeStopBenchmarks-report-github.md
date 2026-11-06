```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Cascade-Stop |   215.4 ns | 1.14 ns | 1.06 ns | 0.0420 |     704 B |
| FV-Cascade-Stop  | 1,449.5 ns | 6.31 ns | 5.59 ns | 0.2041 |    3424 B |
