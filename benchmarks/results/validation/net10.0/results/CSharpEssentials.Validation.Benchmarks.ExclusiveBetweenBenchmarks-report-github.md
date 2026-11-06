```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-ExclusiveBetween |  60.93 ns | 0.375 ns | 0.351 ns | 0.0095 |     160 B |
| FV-ExclusiveBetween  | 111.50 ns | 1.154 ns | 1.023 ns | 0.0358 |     600 B |
