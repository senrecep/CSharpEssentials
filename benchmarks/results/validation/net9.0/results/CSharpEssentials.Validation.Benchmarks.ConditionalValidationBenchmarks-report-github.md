```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 285.9 ns | 1.65 ns | 1.54 ns | 0.0339 |     568 B |
| FV-Conditional-Business  | 311.7 ns | 1.38 ns | 1.29 ns | 0.0415 |     696 B |
| CSE-Conditional-Personal | 250.9 ns | 0.71 ns | 0.66 ns | 0.0243 |     408 B |
| FV-Conditional-Personal  | 278.4 ns | 0.99 ns | 0.88 ns | 0.0396 |     664 B |
