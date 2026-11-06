```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method   | Mean         | Error      | StdDev     | Gen0   | Allocated |
|--------- |-------------:|-----------:|-----------:|-------:|----------:|
| CSE-Ctor |     7.015 ns |  0.0371 ns |  0.0290 ns | 0.0014 |      24 B |
| FV-Ctor  | 4,734.993 ns | 20.3887 ns | 17.0255 ns | 0.5188 |    9096 B |
