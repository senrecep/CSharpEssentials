```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                | Mean     | Error   | StdDev  | Gen0   | Allocated |
|---------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Regex-Precompiled | 260.2 ns | 0.42 ns | 0.37 ns | 0.0143 |     240 B |
| FV-Regex-Precompiled  | 403.9 ns | 1.46 ns | 1.37 ns | 0.0377 |     632 B |
