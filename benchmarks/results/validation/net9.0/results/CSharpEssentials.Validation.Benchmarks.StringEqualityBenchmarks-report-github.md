```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method       | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Equality | 158.7 ns | 1.22 ns | 1.14 ns | 0.0229 |     384 B |
| FV-Equality  | 221.4 ns | 2.85 ns | 2.66 ns | 0.0377 |     632 B |
