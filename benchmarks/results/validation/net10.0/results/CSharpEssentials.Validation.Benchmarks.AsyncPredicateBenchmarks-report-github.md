```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method        | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|-------:|----------:|
| CSE-MustAsync | 1.152 μs | 0.0211 μs | 0.0197 μs | 0.0496 |     847 B |
| FV-MustAsync  | 1.368 μs | 0.0217 μs | 0.0203 μs | 0.0858 |    1463 B |
