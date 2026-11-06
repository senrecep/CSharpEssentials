```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| CSE-LargeCollection | 10.75 μs | 0.059 μs | 0.055 μs | 1.5564 |  25.45 KB |
| FV-LargeCollection  | 29.22 μs | 0.330 μs | 0.309 μs | 3.0212 |  49.36 KB |
