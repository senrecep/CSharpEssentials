```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method               | Mean       | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-Cascade-Continue |   349.7 ns |  3.19 ns |  2.98 ns | 0.0610 |      1 KB |
| FV-Cascade-Continue  | 2,444.5 ns | 16.23 ns | 15.19 ns | 0.3471 |    5.7 KB |
