```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Cascade-Stop |   217.3 ns | 1.33 ns | 1.24 ns | 0.0381 |     640 B |
| FV-Cascade-Stop  | 1,307.3 ns | 6.07 ns | 5.68 ns | 0.2003 |    3360 B |
