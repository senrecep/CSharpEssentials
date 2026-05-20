```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method              | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |   668.6 ns |  2.65 ns |  2.21 ns | 0.2518 |      - |   2.06 KB |
| FV-Complex-Valid    | 1,871.0 ns | 18.28 ns | 16.21 ns | 0.4406 | 0.0019 |    3.6 KB |
| CSE-Complex-Invalid | 1,114.6 ns |  2.66 ns |  2.36 ns | 0.8106 | 0.0095 |   6.63 KB |
| FV-Complex-Invalid  | 7,434.1 ns | 74.24 ns | 61.99 ns | 2.7313 | 0.0839 |  22.32 KB |
