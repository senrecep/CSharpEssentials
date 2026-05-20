```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   530.1 ns |  1.12 ns |  0.88 ns | 0.2265 |      - |    1896 B |
| FV-Wide-Valid    |   821.3 ns |  1.11 ns |  0.92 ns | 0.1173 |      - |     984 B |
| CSE-Wide-Invalid |   711.6 ns |  1.32 ns |  1.03 ns | 0.5665 | 0.0038 |    4744 B |
| FV-Wide-Invalid  | 4,956.0 ns | 27.54 ns | 22.99 ns | 1.7548 | 0.0305 |   14720 B |
