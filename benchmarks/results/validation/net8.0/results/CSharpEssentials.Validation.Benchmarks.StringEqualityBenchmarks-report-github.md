```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method       | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Equality | 152.0 ns | 0.57 ns | 0.53 ns | 0.0229 |     384 B |
| FV-Equality  | 245.2 ns | 1.92 ns | 1.70 ns | 0.0377 |     632 B |
