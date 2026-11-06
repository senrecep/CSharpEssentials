```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Gen0   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Must-Valid   |  52.54 ns | 0.239 ns | 0.224 ns | 0.0100 |     168 B |
| FV-Must-Valid    | 108.38 ns | 0.630 ns | 0.589 ns | 0.0358 |     600 B |
| CSE-Must-Invalid |  88.57 ns | 0.575 ns | 0.537 ns | 0.0215 |     360 B |
| FV-Must-Invalid  | 269.22 ns | 1.536 ns | 1.437 ns | 0.0596 |    1000 B |
