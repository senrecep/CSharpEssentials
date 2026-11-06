```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Mean      | Error    | StdDev   | Gen0   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Must-Valid   |  83.25 ns | 0.708 ns | 0.553 ns | 0.0138 |     232 B |
| FV-Must-Valid    | 147.22 ns | 0.598 ns | 0.500 ns | 0.0358 |     600 B |
| CSE-Must-Invalid | 229.53 ns | 0.821 ns | 0.768 ns | 0.0281 |     472 B |
| FV-Must-Invalid  | 340.36 ns | 2.397 ns | 2.243 ns | 0.0601 |    1008 B |
