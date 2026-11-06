```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 273.4 ns | 1.68 ns | 1.57 ns | 0.0300 |     504 B |
| FV-Conditional-Business  | 255.4 ns | 1.35 ns | 1.26 ns | 0.0415 |     696 B |
| CSE-Conditional-Personal | 213.3 ns | 1.21 ns | 1.13 ns | 0.0205 |     344 B |
| FV-Conditional-Personal  | 259.5 ns | 1.86 ns | 1.56 ns | 0.0396 |     664 B |
