```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                | Mean     | Error   | StdDev  | Gen0   | Allocated |
|---------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Regex-Precompiled | 205.8 ns | 0.69 ns | 0.65 ns | 0.0105 |     176 B |
| FV-Regex-Precompiled  | 320.6 ns | 2.33 ns | 2.07 ns | 0.0377 |     632 B |
