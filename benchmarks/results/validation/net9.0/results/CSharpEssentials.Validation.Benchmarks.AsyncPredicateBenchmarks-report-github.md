```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method        | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|-------:|----------:|
| CSE-MustAsync | 1.181 μs | 0.0198 μs | 0.0185 μs | 0.0534 |     911 B |
| FV-MustAsync  | 1.426 μs | 0.0175 μs | 0.0164 μs | 0.0858 |    1463 B |
