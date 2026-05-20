```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method             | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Simple-Valid   |   184.5 ns |  1.40 ns |  1.17 ns | 0.0725 |      - |     608 B |
| FV-Simple-Valid    |   313.9 ns |  1.42 ns |  1.19 ns | 0.0839 |      - |     704 B |
| CSE-Simple-Invalid |   260.1 ns |  3.23 ns |  3.02 ns | 0.1845 |      - |    1544 B |
| FV-Simple-Invalid  | 2,113.1 ns | 14.28 ns | 11.92 ns | 0.8659 | 0.0076 |    7264 B |
