```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method   | Mean         | Error      | StdDev     | Gen0   | Allocated |
|--------- |-------------:|-----------:|-----------:|-------:|----------:|
| CSE-Ctor |     5.422 ns |  0.0332 ns |  0.0311 ns | 0.0014 |      24 B |
| FV-Ctor  | 3,499.912 ns | 16.4439 ns | 14.5770 ns | 0.5188 |    9000 B |
