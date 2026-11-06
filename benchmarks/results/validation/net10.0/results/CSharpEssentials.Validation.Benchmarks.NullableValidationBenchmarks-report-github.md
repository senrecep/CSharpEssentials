```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method               | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-Nullable-Valid   |   110.7 ns | 0.30 ns | 0.28 ns | 0.0157 |     264 B |
| FV-Nullable-Valid    |   213.3 ns | 0.76 ns | 0.72 ns | 0.0391 |     656 B |
| CSE-Nullable-Invalid |   199.9 ns | 1.84 ns | 1.53 ns | 0.0463 |     776 B |
| FV-Nullable-Invalid  | 1,379.6 ns | 9.71 ns | 9.08 ns | 0.1698 |    2848 B |
