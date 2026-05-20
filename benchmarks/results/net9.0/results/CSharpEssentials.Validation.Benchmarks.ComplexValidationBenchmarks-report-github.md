```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method              | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Complex-Valid   |   788.4 ns |  2.96 ns |  2.47 ns | 0.3595 |      - |   2.94 KB |
| FV-Complex-Valid    | 2,099.0 ns | 18.59 ns | 16.48 ns | 0.4387 |      - |    3.6 KB |
| CSE-Complex-Invalid | 1,367.3 ns |  4.08 ns |  3.41 ns | 0.8793 | 0.0095 |    7.2 KB |
| FV-Complex-Invalid  | 8,529.4 ns | 70.62 ns | 62.60 ns | 2.8381 | 0.0763 |   23.2 KB |
