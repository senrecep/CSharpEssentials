```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                 | Mean       | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------------------- |-----------:|--------:|--------:|-------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   143.5 ns | 0.72 ns | 0.60 ns | 0.0305 |      - |     256 B |
| FV-MultiRegex-Valid    |   336.2 ns | 1.48 ns | 1.23 ns | 0.0753 |      - |     632 B |
| CSE-MultiRegex-Invalid |   203.9 ns | 0.53 ns | 0.44 ns | 0.1242 | 0.0002 |    1040 B |
| FV-MultiRegex-Invalid  | 1,298.5 ns | 9.20 ns | 7.68 ns | 0.5589 | 0.0057 |    4680 B |
