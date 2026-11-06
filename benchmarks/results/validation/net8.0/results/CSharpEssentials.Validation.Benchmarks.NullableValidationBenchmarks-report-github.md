```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method               | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Nullable-Valid   |   182.5 ns | 0.72 ns | 0.67 ns | 0.0234 |     392 B |
| FV-Nullable-Valid    |   327.7 ns | 1.02 ns | 0.95 ns | 0.0358 |     600 B |
| CSE-Nullable-Invalid |   324.4 ns | 1.29 ns | 1.21 ns | 0.0539 |     904 B |
| FV-Nullable-Invalid  | 1,913.0 ns | 3.94 ns | 3.68 ns | 0.1774 |    2976 B |
