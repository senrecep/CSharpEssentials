```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| CSE-LargeCollection | 15.56 μs | 0.051 μs | 0.045 μs | 2.1973 |     36 KB |
| FV-LargeCollection  | 36.95 μs | 0.098 μs | 0.082 μs | 3.0518 |  50.14 KB |
