```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |  1.799 μs | 0.0058 μs | 0.0054 μs | 0.1774 |      - |   2.91 KB |
| FV-Complex-Valid    |  3.707 μs | 0.0172 μs | 0.0161 μs | 0.2289 |      - |   3.79 KB |
| CSE-Complex-Invalid |  3.343 μs | 0.0113 μs | 0.0105 μs | 0.4425 | 0.0038 |   7.27 KB |
| FV-Complex-Invalid  | 15.710 μs | 0.0627 μs | 0.0586 μs | 1.4038 | 0.0305 |   23.2 KB |
