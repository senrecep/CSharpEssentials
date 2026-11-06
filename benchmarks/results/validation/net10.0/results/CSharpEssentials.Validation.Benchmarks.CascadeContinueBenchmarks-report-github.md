```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method               | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Cascade-Continue |   364.2 ns |  2.23 ns |  2.09 ns | 0.0610 |      - |      1 KB |
| FV-Cascade-Continue  | 2,551.7 ns | 12.19 ns | 10.81 ns | 0.3471 | 0.0038 |   5.72 KB |
