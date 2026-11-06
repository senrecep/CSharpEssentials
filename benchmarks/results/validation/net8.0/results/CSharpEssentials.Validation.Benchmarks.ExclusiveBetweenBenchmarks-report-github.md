```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-ExclusiveBetween |  90.35 ns | 0.468 ns | 0.366 ns | 0.0134 |     224 B |
| FV-ExclusiveBetween  | 148.95 ns | 1.250 ns | 1.169 ns | 0.0358 |     600 B |
