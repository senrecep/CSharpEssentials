```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method         | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Comparable |  89.79 ns | 0.382 ns | 0.357 ns | 0.0134 |     224 B |
| FV-Comparable  | 203.32 ns | 0.817 ns | 0.764 ns | 0.0358 |     600 B |
