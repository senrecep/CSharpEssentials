```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-ExclusiveBetween |  61.18 ns | 0.293 ns | 0.260 ns | 0.0095 |     160 B |
| FV-ExclusiveBetween  | 110.50 ns | 1.589 ns | 1.486 ns | 0.0358 |     600 B |
