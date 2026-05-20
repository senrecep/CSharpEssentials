```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Wide-Valid   |   393.2 ns |  4.82 ns |  4.51 ns | 0.2036 |      - |    1704 B |
| FV-Wide-Valid    |   686.9 ns |  1.18 ns |  0.99 ns | 0.1173 |      - |     984 B |
| CSE-Wide-Invalid |   653.0 ns |  3.46 ns |  2.89 ns | 0.5512 | 0.0038 |    4616 B |
| FV-Wide-Invalid  | 4,416.9 ns | 19.22 ns | 17.98 ns | 1.6632 | 0.0305 |   13952 B |
