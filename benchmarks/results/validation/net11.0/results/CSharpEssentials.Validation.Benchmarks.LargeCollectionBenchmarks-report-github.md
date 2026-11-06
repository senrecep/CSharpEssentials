```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| CSE-LargeCollection | 10.22 μs | 0.052 μs | 0.048 μs | 1.5564 |  25.45 KB |
| FV-LargeCollection  | 26.82 μs | 0.119 μs | 0.112 μs | 3.0212 |  49.36 KB |
