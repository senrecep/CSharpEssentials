```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method              | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |   640.9 ns |  0.70 ns |  0.54 ns | 0.2518 |      - |   2.06 KB |
| FV-Complex-Valid    | 1,859.6 ns |  9.07 ns |  7.08 ns | 0.4406 | 0.0019 |    3.6 KB |
| CSE-Complex-Invalid | 1,136.4 ns | 12.19 ns | 11.40 ns | 0.8106 | 0.0095 |   6.63 KB |
| FV-Complex-Invalid  | 7,622.4 ns | 86.65 ns | 76.81 ns | 2.7313 | 0.0839 |  22.32 KB |
