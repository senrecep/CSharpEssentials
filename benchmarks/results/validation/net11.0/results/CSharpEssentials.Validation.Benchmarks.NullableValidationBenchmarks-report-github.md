```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method               | Mean       | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-Nullable-Valid   |   106.9 ns |  0.44 ns |  0.41 ns | 0.0157 |     264 B |
| FV-Nullable-Valid    |   218.7 ns |  1.50 ns |  1.33 ns | 0.0391 |     656 B |
| CSE-Nullable-Invalid |   199.3 ns |  1.27 ns |  1.13 ns | 0.0463 |     776 B |
| FV-Nullable-Invalid  | 1,379.8 ns | 19.93 ns | 17.67 ns | 0.1678 |    2832 B |
