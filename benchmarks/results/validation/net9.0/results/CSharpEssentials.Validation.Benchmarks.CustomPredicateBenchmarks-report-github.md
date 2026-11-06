```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Gen0   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Must-Valid   |  81.30 ns | 0.337 ns | 0.299 ns | 0.0138 |     232 B |
| FV-Must-Valid    | 145.76 ns | 0.924 ns | 0.865 ns | 0.0358 |     600 B |
| CSE-Must-Invalid | 124.01 ns | 1.316 ns | 1.231 ns | 0.0253 |     424 B |
| FV-Must-Invalid  | 344.63 ns | 1.325 ns | 1.240 ns | 0.0601 |    1008 B |
