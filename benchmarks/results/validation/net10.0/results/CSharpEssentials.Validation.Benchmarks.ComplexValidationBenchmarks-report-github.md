```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |  1.281 μs | 0.0060 μs | 0.0057 μs | 0.1240 |      - |   2.05 KB |
| FV-Complex-Valid    |  3.150 μs | 0.0131 μs | 0.0116 μs | 0.2213 |      - |   3.66 KB |
| CSE-Complex-Invalid |  2.538 μs | 0.0177 μs | 0.0166 μs | 0.4082 | 0.0038 |   6.69 KB |
| FV-Complex-Invalid  | 13.495 μs | 0.0642 μs | 0.0601 μs | 1.3580 | 0.0305 |  22.35 KB |
