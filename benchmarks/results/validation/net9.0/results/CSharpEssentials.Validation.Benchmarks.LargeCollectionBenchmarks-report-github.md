```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| CSE-LargeCollection | 14.51 μs | 0.068 μs | 0.064 μs | 2.1210 |  34.83 KB |
| FV-LargeCollection  | 32.75 μs | 0.073 μs | 0.068 μs | 2.8076 |  46.63 KB |
