```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                  | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------------ |-----------:|--------:|--------:|-------:|----------:|
| CSE-LengthRange-Valid   |   161.7 ns | 0.76 ns | 0.71 ns | 0.0224 |     376 B |
| FV-LengthRange-Valid    |   188.7 ns | 1.03 ns | 0.96 ns | 0.0358 |     600 B |
| CSE-LengthRange-Invalid |   399.1 ns | 1.46 ns | 1.37 ns | 0.0572 |     960 B |
| FV-LengthRange-Invalid  | 1,790.3 ns | 8.82 ns | 8.25 ns | 0.2308 |    3872 B |
