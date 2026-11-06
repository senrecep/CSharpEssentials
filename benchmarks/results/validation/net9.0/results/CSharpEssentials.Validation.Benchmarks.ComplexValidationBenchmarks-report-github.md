```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |  1.734 μs | 0.0061 μs | 0.0057 μs | 0.1736 |      - |   2.86 KB |
| FV-Complex-Valid    |  3.386 μs | 0.0066 μs | 0.0062 μs | 0.2174 |      - |    3.6 KB |
| CSE-Complex-Invalid |  2.983 μs | 0.0136 μs | 0.0127 μs | 0.4349 | 0.0038 |   7.13 KB |
| FV-Complex-Invalid  | 15.531 μs | 0.0955 μs | 0.0847 μs | 1.4038 | 0.0305 |   23.2 KB |
