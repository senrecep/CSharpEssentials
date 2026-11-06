```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                | Mean     | Error   | StdDev  | Gen0   | Allocated |
|---------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Regex-Precompiled | 238.4 ns | 0.55 ns | 0.49 ns | 0.0143 |     240 B |
| FV-Regex-Precompiled  | 371.7 ns | 1.89 ns | 1.67 ns | 0.0377 |     632 B |
