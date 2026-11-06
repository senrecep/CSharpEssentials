```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-ExclusiveBetween |  80.92 ns | 0.325 ns | 0.304 ns | 0.0134 |     224 B |
| FV-ExclusiveBetween  | 152.29 ns | 0.700 ns | 0.654 ns | 0.0358 |     600 B |
