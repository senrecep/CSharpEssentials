```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method               | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Nullable-Valid   |   229.8 ns | 0.51 ns | 0.47 ns | 0.0234 |     392 B |
| FV-Nullable-Valid    |   288.1 ns | 1.99 ns | 1.86 ns | 0.0358 |     600 B |
| CSE-Nullable-Invalid |   285.0 ns | 3.21 ns | 3.00 ns | 0.0539 |     904 B |
| FV-Nullable-Invalid  | 1,689.4 ns | 5.17 ns | 4.58 ns | 0.1774 |    2976 B |
