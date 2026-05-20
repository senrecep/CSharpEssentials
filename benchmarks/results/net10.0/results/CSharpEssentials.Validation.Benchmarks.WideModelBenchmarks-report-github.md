```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   388.1 ns |  2.35 ns |  2.08 ns | 0.2036 |      - |    1704 B |
| FV-Wide-Valid    |   694.4 ns |  4.29 ns |  3.58 ns | 0.1173 |      - |     984 B |
| CSE-Wide-Invalid |   635.5 ns |  1.64 ns |  1.37 ns | 0.5512 | 0.0038 |    4616 B |
| FV-Wide-Invalid  | 4,449.0 ns | 60.50 ns | 56.59 ns | 1.6632 | 0.0305 |   13952 B |
