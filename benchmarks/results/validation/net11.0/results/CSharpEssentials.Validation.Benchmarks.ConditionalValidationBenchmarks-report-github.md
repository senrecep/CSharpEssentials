```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 251.1 ns | 0.76 ns | 0.71 ns | 0.0291 |     488 B |
| FV-Conditional-Business  | 259.6 ns | 1.25 ns | 1.10 ns | 0.0415 |     696 B |
| CSE-Conditional-Personal | 195.3 ns | 0.76 ns | 0.71 ns | 0.0200 |     336 B |
| FV-Conditional-Personal  | 247.0 ns | 1.63 ns | 1.45 ns | 0.0396 |     664 B |
