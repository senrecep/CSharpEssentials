```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------------ |-----------:|--------:|--------:|-------:|----------:|
| CSE-LengthRange-Valid   |   119.3 ns | 0.96 ns | 0.90 ns | 0.0186 |     312 B |
| FV-LengthRange-Valid    |   143.4 ns | 1.97 ns | 1.84 ns | 0.0358 |     600 B |
| CSE-LengthRange-Invalid |   274.7 ns | 1.63 ns | 1.52 ns | 0.0458 |     768 B |
| FV-LengthRange-Invalid  | 1,499.0 ns | 7.27 ns | 6.80 ns | 0.2270 |    3808 B |
