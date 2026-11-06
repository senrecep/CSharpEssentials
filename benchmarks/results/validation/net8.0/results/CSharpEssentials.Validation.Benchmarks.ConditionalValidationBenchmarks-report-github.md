```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 301.7 ns | 0.77 ns | 0.72 ns | 0.0339 |     568 B |
| FV-Conditional-Business  | 347.2 ns | 2.31 ns | 1.93 ns | 0.0415 |     696 B |
| CSE-Conditional-Personal | 236.2 ns | 0.80 ns | 0.71 ns | 0.0243 |     408 B |
| FV-Conditional-Personal  | 316.2 ns | 1.02 ns | 0.90 ns | 0.0396 |     664 B |
