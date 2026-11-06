```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Cascade-Stop |   292.2 ns | 1.67 ns | 1.57 ns | 0.0448 |     752 B |
| FV-Cascade-Stop  | 1,506.0 ns | 7.76 ns | 7.26 ns | 0.2041 |    3424 B |
