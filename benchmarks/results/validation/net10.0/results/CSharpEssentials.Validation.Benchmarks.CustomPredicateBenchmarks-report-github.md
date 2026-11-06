```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Gen0   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Must-Valid   |  53.08 ns | 0.396 ns | 0.370 ns | 0.0100 |     168 B |
| FV-Must-Valid    | 117.78 ns | 0.647 ns | 0.605 ns | 0.0358 |     600 B |
| CSE-Must-Invalid |  94.68 ns | 0.755 ns | 0.707 ns | 0.0215 |     360 B |
| FV-Must-Invalid  | 277.31 ns | 2.519 ns | 2.233 ns | 0.0601 |    1008 B |
