```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method               | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Cascade-Continue |   492.9 ns |  3.45 ns |  3.22 ns | 0.0677 |      - |   1.11 KB |
| FV-Cascade-Continue  | 3,267.3 ns | 13.24 ns | 12.39 ns | 0.3586 | 0.0038 |   5.91 KB |
