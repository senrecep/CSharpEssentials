```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                 | Mean       | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------------------- |-----------:|--------:|--------:|-------:|-------:|----------:|
| CSE-MultiRegex-Valid   |   141.5 ns | 0.40 ns | 0.33 ns | 0.0305 |      - |     256 B |
| FV-MultiRegex-Valid    |   335.6 ns | 0.72 ns | 0.56 ns | 0.0753 |      - |     632 B |
| CSE-MultiRegex-Invalid |   209.7 ns | 2.27 ns | 2.13 ns | 0.1242 | 0.0002 |    1040 B |
| FV-MultiRegex-Invalid  | 1,286.8 ns | 6.73 ns | 5.97 ns | 0.5589 | 0.0057 |    4680 B |
