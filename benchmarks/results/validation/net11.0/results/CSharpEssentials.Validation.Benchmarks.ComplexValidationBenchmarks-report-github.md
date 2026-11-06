```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |  1.251 μs | 0.0049 μs | 0.0041 μs | 0.1240 |      - |   2.03 KB |
| FV-Complex-Valid    |  3.012 μs | 0.0093 μs | 0.0078 μs | 0.2213 |      - |   3.66 KB |
| CSE-Complex-Invalid |  2.273 μs | 0.0107 μs | 0.0100 μs | 0.4044 | 0.0038 |   6.64 KB |
| FV-Complex-Invalid  | 12.222 μs | 0.0670 μs | 0.0627 μs | 1.3580 | 0.0305 |  22.24 KB |
