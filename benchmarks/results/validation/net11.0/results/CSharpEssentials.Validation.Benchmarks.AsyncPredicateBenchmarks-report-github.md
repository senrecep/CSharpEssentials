```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method        | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|-------:|----------:|
| CSE-MustAsync | 3.483 μs | 0.0325 μs | 0.0304 μs | 0.0458 |     847 B |
| FV-MustAsync  | 3.559 μs | 0.0225 μs | 0.0188 μs | 0.0839 |    1462 B |
