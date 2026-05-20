```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                 | Mean       | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------------------- |-----------:|--------:|--------:|-------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   163.8 ns | 1.27 ns | 1.13 ns | 0.0381 |      - |     320 B |
| FV-MultiRegex-Valid    |   377.8 ns | 1.53 ns | 1.28 ns | 0.0753 |      - |     632 B |
| CSE-MultiRegex-Invalid |   238.6 ns | 1.37 ns | 1.21 ns | 0.1316 |      - |    1104 B |
| FV-MultiRegex-Invalid  | 1,462.4 ns | 6.29 ns | 5.25 ns | 0.5817 | 0.0038 |    4872 B |
